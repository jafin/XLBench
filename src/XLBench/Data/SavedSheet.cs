using System.Globalization;
using System.IO.Compression;
using System.Xml.Linq;

namespace XLBench.Data;

/// <summary>
/// Reads one column out of a saved <c>.xlsx</c> straight from the package XML — the zip entry, the
/// <c>&lt;c&gt;</c> element, its <c>&lt;f&gt;</c> and its cached <c>&lt;v&gt;</c>.
///
/// <para>Deliberately no library. Checking a library's output by re-opening it with a library —
/// even a neutral one — risks the reader evaluating the formula and reporting the answer the
/// checker wanted rather than the one stored in the file. The scenario's claim is that the totals
/// are right <i>on save</i>, so the check reads what was saved.</para>
/// </summary>
public static class SavedSheet
{
    private static readonly XNamespace Main =
        "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

    private static readonly XNamespace Relationships =
        "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    private static readonly XNamespace PackageRelationships =
        "http://schemas.openxmlformats.org/package/2006/relationships";

    /// <summary>One cell as the file stores it.</summary>
    /// <param name="Formula">Formula text without the leading '=', or null for a literal cell.</param>
    /// <param name="RawValue">Cached value exactly as written, or null when the cell has none.</param>
    /// <param name="Type">The <c>t</c> attribute, e.g. "e" for an error result; null means numeric.</param>
    public readonly record struct Cell(string? Formula, string? RawValue, string? Type)
    {
        /// <summary>The cached value as a number, or null when it is absent or not numeric.</summary>
        public double? Number =>
            Type is null or "n"
            && RawValue is not null
            && double.TryParse(RawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)
                ? d
                : null;
    }

    /// <summary>
    /// Reads <paramref name="column"/> from the workbook's first worksheet, keyed by 1-based sheet
    /// row. Rows with no cell in that column are simply absent from the result.
    /// </summary>
    /// <param name="path">Path to a saved .xlsx.</param>
    /// <param name="column">1-based column index.</param>
    public static Dictionary<int, Cell> ReadColumn(string path, int column)
    {
        using var zip = ZipFile.OpenRead(path);

        var entry = zip.Entries
                        .Where(e => e.FullName.StartsWith("xl/worksheets/sheet", StringComparison.OrdinalIgnoreCase)
                                    && e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        .OrderBy(e => e.FullName, StringComparer.Ordinal)
                        .FirstOrDefault()
                    ?? throw new InvalidOperationException($"'{path}' contains no worksheet part.");

        return ReadColumn(path, entry, column);
    }

    /// <summary>
    /// Reads <paramref name="column"/> from the worksheet named <paramref name="sheetName"/>, keyed
    /// by 1-based sheet row. The part is found the way a reader has to find it — the sheet's
    /// relationship id in <c>xl/workbook.xml</c>, resolved through
    /// <c>xl/_rels/workbook.xml.rels</c> — since libraries number their worksheet parts as they like.
    /// </summary>
    /// <param name="path">Path to a saved .xlsx.</param>
    /// <param name="column">1-based column index.</param>
    /// <param name="sheetName">The worksheet's name as shown on its tab.</param>
    public static Dictionary<int, Cell> ReadColumn(string path, int column, string sheetName)
    {
        using var zip = ZipFile.OpenRead(path);

        var relationshipId = LoadEntry(zip, "xl/workbook.xml", path)
                                 .Root?.Element(Main + "sheets")?.Elements(Main + "sheet")
                                 .FirstOrDefault(s => string.Equals(
                                     (string?)s.Attribute("name"), sheetName, StringComparison.OrdinalIgnoreCase))
                                 ?.Attribute(Relationships + "id")?.Value
                             ?? throw new InvalidOperationException($"'{path}' has no sheet named '{sheetName}'.");

        var target = LoadEntry(zip, "xl/_rels/workbook.xml.rels", path)
                         .Root?.Elements(PackageRelationships + "Relationship")
                         .FirstOrDefault(r => (string?)r.Attribute("Id") == relationshipId)
                         ?.Attribute("Target")?.Value
                     ?? throw new InvalidOperationException(
                         $"'{path}' has no relationship '{relationshipId}' for sheet '{sheetName}'.");

        // Targets are normally relative to xl/ ("worksheets/sheet2.xml") but may be absolute ("/xl/…").
        var entryName = target.StartsWith('/') ? target.TrimStart('/') : $"xl/{target}";
        var entry = zip.GetEntry(entryName)
                    ?? throw new InvalidOperationException($"'{path}' is missing part '{entryName}'.");

        return ReadColumn(path, entry, column);
    }

    private static XDocument LoadEntry(ZipArchive zip, string name, string path)
    {
        var entry = zip.GetEntry(name)
                    ?? throw new InvalidOperationException($"'{path}' is missing part '{name}'.");
        using var stream = entry.Open();
        return XDocument.Load(stream);
    }

    private static Dictionary<int, Cell> ReadColumn(string path, ZipArchiveEntry entry, int column)
    {
        using var stream = entry.Open();
        var doc = XDocument.Load(stream);

        var sheetData = doc.Root?.Element(Main + "sheetData")
                        ?? throw new InvalidOperationException($"'{path}' has no <sheetData>.");

        var result = new Dictionary<int, Cell>();
        foreach (var row in sheetData.Elements(Main + "row"))
        {
            foreach (var cell in row.Elements(Main + "c"))
            {
                if (cell.Attribute("r")?.Value is not { } reference) continue;
                if (ColumnIndex(reference) != column) continue;

                result[RowIndex(reference)] = new Cell(
                    Formula: cell.Element(Main + "f")?.Value,
                    RawValue: cell.Element(Main + "v")?.Value,
                    Type: cell.Attribute("t")?.Value);
            }
        }
        return result;
    }

    /// <summary>1-based column index from an A1 cell reference, e.g. "AB12" -> 28.</summary>
    private static int ColumnIndex(string reference)
    {
        var col = 0;
        foreach (var ch in reference)
        {
            if (!char.IsAsciiLetterUpper(ch)) break;
            col = (col * 26) + (ch - 'A' + 1);
        }
        return col;
    }

    /// <summary>1-based row index from an A1 cell reference, e.g. "AB12" -> 12.</summary>
    private static int RowIndex(string reference)
    {
        var digits = reference.AsSpan().TrimStart("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        return int.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out var row) ? row : 0;
    }
}
