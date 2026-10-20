using BenchmarkDotNet.Attributes;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using XLBench.Data;

namespace XLBench.Benchmarks.MultiSheet;

/// <summary>
/// OpenXML SDK multi-sheet read: the shared string table is materialized once, then every
/// worksheet part is streamed with a SAX-style <see cref="OpenXmlReader"/>, as in
/// <see cref="Read.OpenXmlReadBenchmarks"/>.
///
/// <para>A rich-text shared string is a <c>&lt;si&gt;</c> of <c>&lt;r&gt;</c> runs, and
/// <c>InnerText</c> joins their <c>&lt;t&gt;</c> text, so the table resolves straight to plain
/// text. The styles part is never parsed: nothing the read returns depends on it, and the SDK only
/// loads the parts it is asked for.</para>
/// </summary>
public class OpenXmlSheetsBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        SheetsData.EnsureLoaded();
        _bytes = SheetsData.SourceXlsx;
    }

    [Benchmark]
    public long OpenAndReadAllSheets() => Read(_bytes);

    /// <inheritdoc cref="ClosedXmlSheetsBenchmarks.Read"/>
    internal static long Read(byte[] bytes)
    {
        using var doc = SpreadsheetDocument.Open(new MemoryStream(bytes), false);
        var wbPart = doc.WorkbookPart!;

        var shared = wbPart.SharedStringTablePart?.SharedStringTable is { } sst
            ? sst.Elements<SharedStringItem>().Select(si => si.InnerText).ToArray()
            : [];

        long cells = 0;
        long textChars = 0;
        foreach (var wsPart in wbPart.WorksheetParts)
        {
            using var reader = OpenXmlReader.Create(wsPart);
            while (reader.Read())
            {
                if (reader.ElementType != typeof(Cell) || !reader.IsStartElement) continue;
                var cell = (Cell)reader.LoadCurrentElement()!;

                cells++;
                textChars += TextLength(cell, shared);
            }
        }
        return SheetsData.Checksum(cells, textChars);
    }

    /// <summary>Plain-text length of a text cell; 0 for a number or date.</summary>
    private static int TextLength(Cell cell, string[] shared)
    {
        var type = cell.DataType?.Value;
        if (type == CellValues.SharedString
            && int.TryParse(cell.CellValue?.InnerText, out var idx)
            && idx >= 0 && idx < shared.Length)
        {
            return shared[idx].Length;
        }
        if (type == CellValues.InlineString)
            return cell.InlineString?.InnerText.Length ?? 0;
        return 0;
    }
}
