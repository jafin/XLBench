using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using XLBench.Benchmarks.Template;
using XLBench.Libraries;

namespace XLBench.Data;

/// <summary>
/// Runs every library's <c>FillTemplateAndSave</c> once, unmeasured, writing the filled workbook
/// to <c>output/</c>, then reports what of the template survived the round trip
/// (<c>dotnet run -- template</c>).
///
/// <para>Two kinds of result come out of it, and they are kept apart. <b>Data</b> — the named
/// cells hold what was written, all <see cref="TemplateData.RowCount"/> rows are present with the
/// right values and Amount formulas, and the table covers them — is pass/fail: a library that gets
/// it wrong produced a wrong file. <b>Fidelity</b> — the chart, the validation, the conditional
/// formatting, the merged title, the named styles, the column formats on the new rows, the
/// structured references in the summary block — is reported, not failed: losing a template
/// feature is a real finding about the library, but the work measured was the same.</para>
///
/// <para>Like <see cref="InsertArtifacts"/>, the check reads the saved package XML directly
/// rather than re-opening it with a library, so it reports what was written and not what some
/// reader chose to reconstruct. It also runs the OpenXML SDK's schema validator over each file and
/// reports the error count; Excel is more forgiving than the schema, so a non-zero count is a
/// warning, not a verdict.</para>
/// </summary>
public static class TemplateArtifacts
{
    private static readonly XNamespace Main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace Rel = "http://schemas.openxmlformats.org/package/2006/relationships";
    private static readonly XNamespace OfficeRel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace Chart = "http://schemas.openxmlformats.org/drawingml/2006/chart";
    private static readonly XNamespace X14 = "http://schemas.microsoft.com/office/spreadsheetml/2009/9/main";
    private static readonly XNamespace Xm = "http://schemas.microsoft.com/office/excel/2006/main";

    private const string TableRelType = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/table";
    private const string DrawingRelType = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/drawing";
    private const string ChartRelType = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/chart";
    private const string SharedStringsRelType = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings";
    private const string StylesRelType = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles";

    /// <summary>What the validator reports for the untouched template — the baseline each artifact is held to.</summary>
    private static readonly Lazy<List<string>> TemplateSchemaErrors =
        new(() => Validate(new MemoryStream(TemplateData.TemplateXlsx, writable: false)));

    /// <summary>The named cell styles the template defines in <c>styles.xml</c>.</summary>
    private static readonly string[] TemplateStyles = ["Report Title", "Report Date", "Report Currency", "Report Percent"];

    public static void WriteAll()
    {
        TemplateData.EnsureLoaded();

        Console.WriteLine(
            $"[XLBench] Template scenario: fill {TemplateData.RowCount} rows into table " +
            $"'{TemplateData.TableName}' ({TemplateData.TemplateTableRef} -> {TemplateData.TableRef}) " +
            "of an Excel-authored template, plus three named header cells.");
        Console.WriteLine(
            $"[XLBench] The template itself has {TemplateSchemaErrors.Value.Count} schema validation error(s) " +
            "(Excel's own output against the SDK's schemas); artifacts are judged on errors beyond those.");
        Console.WriteLine($"[XLBench] Writing template artifacts to {TemplateOutput.Directory}");

        // Each entry is a library's [GlobalCleanup] save, invoked directly. One failing library
        // must not stop the others — a partial set is still reviewable, and the exception text
        // is the useful output.
        (string Library, string Slug, Func<bool> Run)[] writers =
        [
            ("ClosedXML", "closedxml", ClosedXmlTemplateBenchmarks.WriteArtifact),
            ("EPPlus", "epplus", EpPlusTemplateBenchmarks.WriteArtifact),
            ("NPOI", "npoi", NpoiTemplateBenchmarks.WriteArtifact),
            ("OpenXML SDK", "openxml", OpenXmlTemplateBenchmarks.WriteArtifact),
            ("XLibur", "xlibur", XLiburTemplateBenchmarks.WriteArtifact),
            ("OfficeIMO", "officeimo", OfficeImoTemplateBenchmarks.WriteArtifact),
            ("Telerik", "telerik", TelerikTemplateBenchmarks.WriteArtifact),
            ("IronXL", "ironxl", IronXlTemplateBenchmarks.WriteArtifact),
        ];

        // IronXL throws rather than watermarking when unlicensed (see IronXlLicense).
        if (!IronXlLicense.KeyAvailable)
        {
            writers = [.. writers.Where(w => w.Library != "IronXL")];
            Console.WriteLine("[XLBench] Skipping IronXL — XLBENCH_IRONXL_KEY not set (see README).");
        }

        // Telerik watermarks its output when unlicensed (see TelerikLicense).
        if (!TelerikLicense.IsLicensed)
        {
            writers = [.. writers.Where(w => w.Library != "Telerik")];
            Console.WriteLine("[XLBench] Skipping Telerik — no licence found (see README).");
        }

        var written = 0;
        var skipped = 0;
        var failed = 0;
        var wrong = 0;

        foreach (var (library, slug, run) in writers)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                var saved = run();
                sw.Stop();

                if (!saved)
                {
                    skipped++;
                    Console.WriteLine($"[XLBench] {library}: skipped (artifact locked).");
                    continue;
                }

                written++;

                var report = Inspect(TemplateOutput.PathFor(slug));
                if (!report.DataOk) wrong++;

                Console.WriteLine(
                    $"[XLBench] {library}: {(report.DataOk ? "OK" : "WRONG DATA")} — " +
                    $"{sw.ElapsedMilliseconds:N0} ms (single cold pass, not a benchmark result).");
                foreach (var line in report.Lines)
                    Console.WriteLine($"[XLBench]   {line}");
            }
#pragma warning disable S2221 // A broken library must not prevent the others from producing artifacts
            catch (Exception ex)
#pragma warning restore S2221
            {
                failed++;
                Console.WriteLine($"[XLBench] {library} failed: {ex}");
            }
        }

        Console.WriteLine($"[XLBench] Wrote {written} of {writers.Length} template artifact(s)."
            + (skipped > 0 ? $" {skipped} skipped (file locked — close it in Excel and re-run)." : string.Empty)
            + (failed > 0 ? $" {failed} failed." : string.Empty)
            + (wrong > 0 ? $" {wrong} produced wrong data." : string.Empty));
    }

    /// <summary>What the saved file says, as printable lines, plus whether the data itself is right.</summary>
    private sealed record Report(bool DataOk, List<string> Lines);

    private static Report Inspect(string path)
    {
        var lines = new List<string>();
        var dataOk = true;

        void Data(bool ok, string label, string detail)
        {
            if (!ok) dataOk = false;
            lines.Add($"{(ok ? "ok  " : "FAIL")} data     {label}: {detail}");
        }

        void Fidelity(bool ok, string label, string detail) =>
            lines.Add($"{(ok ? "ok  " : "lost")} fidelity {label}: {detail}");

        using var zip = ZipFile.OpenRead(path);
        var pkg = new SavedPackage(zip);

        var workbookPath = pkg.Target("", "_rels/.rels",
            "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument");
        var workbook = pkg.Load(workbookPath)!;
        var sheetElement = workbook.Root!.Element(Main + "sheets")!.Elements(Main + "sheet")
            .First(s => (string?)s.Attribute("name") == TemplateData.SheetName);
        var sheetPath = pkg.TargetById(workbookPath, (string)sheetElement.Attribute(OfficeRel + "id")!);
        var sheet = pkg.Load(sheetPath)!;

        var sharedStrings = pkg.Load(pkg.TargetOfType(workbookPath, SharedStringsRelType)) is { } sst
            ? sst.Root!.Elements(Main + "si").Select(si => string.Concat(si.Descendants(Main + "t").Select(t => t.Value))).ToList()
            : [];
        var styles = pkg.Load(pkg.TargetOfType(workbookPath, StylesRelType));
        var cells = ReadCells(sheet, sharedStrings);

        // ---- Data ---------------------------------------------------------------------------
        Data(Text(cells, TemplateData.TitleCell) == TemplateData.Title, "ReportTitle", Describe(cells, TemplateData.TitleCell));
        Data(NumberAt(cells, TemplateData.DateCell) is { } d && d == TemplateData.ReportDate.ToOADate(),
            "ReportDate", Describe(cells, TemplateData.DateCell));
        Data(Text(cells, TemplateData.RegionCell) == TemplateData.Region, "Region", Describe(cells, TemplateData.RegionCell));

        var (badRows, firstBad) = CheckRows(cells);
        Data(badRows == 0, "rows",
            badRows == 0
                ? $"{TemplateData.RowCount}/{TemplateData.RowCount} rows match (values and Amount formulas)"
                : $"{TemplateData.RowCount - badRows}/{TemplateData.RowCount} rows match; first mismatch: {firstBad}");

        var tablePath = pkg.TargetsOfType(sheetPath, TableRelType)
            .FirstOrDefault(p => (string?)pkg.Load(p)?.Root?.Attribute("name") == TemplateData.TableName);
        if (tablePath is null)
        {
            Fidelity(false, "table", $"'{TemplateData.TableName}' is gone from the saved sheet");
        }
        else
        {
            var table = pkg.Load(tablePath)!.Root!;
            var tableRef = (string?)table.Attribute("ref");
            var filterRef = (string?)table.Element(Main + "autoFilter")?.Attribute("ref");
            Data(tableRef == TemplateData.TableRef && (filterRef is null || filterRef == TemplateData.TableRef),
                "table", $"ref={tableRef ?? "<none>"}, autoFilter={filterRef ?? "<none>"} (expected {TemplateData.TableRef})");
        }

        // ---- Fidelity -----------------------------------------------------------------------
        var names = workbook.Root.Element(Main + "definedNames")?.Elements(Main + "definedName")
                        .ToDictionary(n => (string)n.Attribute("name")!, n => n.Value) ?? [];
        string[] expectedNames = [TemplateData.TitleName, TemplateData.DateName, TemplateData.RegionName];
        var missingNames = expectedNames.Where(n => !names.ContainsKey(n)).ToList();
        Fidelity(missingNames.Count == 0, "defined names",
            missingNames.Count == 0 ? string.Join(", ", expectedNames) : $"missing {string.Join(", ", missingNames)}");

        var merges = sheet.Root!.Element(Main + "mergeCells")?.Elements(Main + "mergeCell")
                         .Select(m => (string?)m.Attribute("ref")).ToList() ?? [];
        Fidelity(merges.Contains(TemplateData.TitleMerge), "merged title",
            merges.Count == 0 ? "no merged cells" : string.Join(", ", merges));

        var validations = sheet.Root.Element(Main + "dataValidations")?.Elements(Main + "dataValidation")
                              .Select(v => (string?)v.Attribute("sqref"))
                              .Concat(sheet.Root.Descendants(X14 + "dataValidation").Select(v => (string?)v.Element(Xm + "sqref")?.Value))
                              .ToList() ?? [];
        Fidelity(Covers(validations, TemplateData.ValidationRange), "data validation",
            validations.Count == 0 ? "none" : string.Join(", ", validations));

        var conditional = sheet.Root.Elements(Main + "conditionalFormatting")
            .Select(c => (string?)c.Attribute("sqref"))
            .Concat(sheet.Root.Descendants(X14 + "conditionalFormatting").Select(c => (string?)c.Element(Xm + "sqref")?.Value))
            .ToList();
        Fidelity(Covers(conditional, TemplateData.ConditionalFormatRange), "conditional formatting",
            conditional.Count == 0 ? "none" : string.Join(", ", conditional));

        var chartRefs = pkg.TargetsOfType(sheetPath, DrawingRelType)
            .SelectMany(drawing => pkg.TargetsOfType(drawing, ChartRelType))
            .SelectMany(chart => pkg.Load(chart)?.Descendants(Chart + "f").Select(f => f.Value) ?? [])
            .ToList();
        var chartOk = chartRefs.Contains(TemplateData.ChartCategories) && chartRefs.Contains(TemplateData.ChartValues);
        Fidelity(chartOk, "chart",
            chartRefs.Count == 0 ? "no chart in the saved file" : $"series refs {string.Join(", ", chartRefs.Distinct())}");

        // The summary block must still be formulas over the table, by name: a structured reference
        // is what lets the totals follow the table as it grows.
        var summaryCells = Enumerable.Range(6, 6).Select(r => $"I{r}").ToList();
        var degraded = summaryCells
            .Where(c => cells.GetValueOrDefault(c).Formula?.Contains($"{TemplateData.TableName}[", StringComparison.Ordinal) != true)
            .ToList();
        Fidelity(degraded.Count == 0, "summary formulas",
            degraded.Count == 0
                ? $"I6:I11 still reference {TemplateData.TableName}[…], e.g. I6 = {cells.GetValueOrDefault("I6").Formula}"
                : $"{degraded.Count}/{summaryCells.Count} no longer reference the table; I6 = {Describe(cells, "I6")}");

        var styleNames = styles?.Root?.Element(Main + "cellStyles")?.Elements(Main + "cellStyle")
                             .Select(s => (string?)s.Attribute("name")).ToHashSet() ?? [];
        var missingStyles = TemplateStyles.Where(s => !styleNames.Contains(s)).ToList();
        Fidelity(missingStyles.Count == 0, "named styles",
            missingStyles.Count == 0 ? string.Join(", ", TemplateStyles) : $"missing {string.Join(", ", missingStyles)}");

        var last = TemplateData.LastDataRow;
        var dateFormat = NumberFormatOf(styles, cells.GetValueOrDefault($"A{last}").Style);
        var priceFormat = NumberFormatOf(styles, cells.GetValueOrDefault($"E{last}").Style);
        var formatsOk = IsDateFormat(dateFormat) && IsCurrencyFormat(priceFormat);
        Fidelity(formatsOk, "new-row formats",
            $"A{last} '{dateFormat ?? "General"}', E{last} '{priceFormat ?? "General"}'");

        // Compared against the template's own result rather than zero: what matters is whether the
        // library introduced errors, not whether Excel's output satisfies the SDK's schema set.
        using var savedStream = File.OpenRead(path);
        var introduced = Validate(savedStream).Except(TemplateSchemaErrors.Value).ToList();
        lines.Add($"{(introduced.Count == 0 ? "ok  " : "warn")} schema   {introduced.Count} validation error(s) beyond the template's own"
                  + (introduced.Count > 0 ? $"; first: {introduced[0]}" : string.Empty));

        return new Report(dataOk, lines);
    }

    /// <summary>
    /// Checks every data row against <see cref="TemplateData.Rows"/>. Returns the number of rows
    /// that differ and a description of the first.
    /// </summary>
    private static (int Bad, string? First) CheckRows(Dictionary<string, SavedCell> cells)
    {
        var bad = 0;
        string? first = null;
        var rows = TemplateData.Rows;
        for (var i = 0; i < rows.Length; i++)
        {
            var r = TemplateData.FirstDataRow + i;
            var line = rows[i];

            string? problem = null;
            if (NumberAt(cells, $"A{r}") is not { } date || Math.Abs(date - line.Date.ToOADate()) > 1e-9)
                problem = $"A{r} {Describe(cells, $"A{r}")}, expected {line.Date:yyyy-MM-dd}";
            else if (Text(cells, $"B{r}") != line.Product)
                problem = $"B{r} {Describe(cells, $"B{r}")}, expected '{line.Product}'";
            else if (Text(cells, $"C{r}") != line.Category)
                problem = $"C{r} {Describe(cells, $"C{r}")}, expected '{line.Category}'";
            else if (NumberAt(cells, $"D{r}") != line.Quantity)
                problem = $"D{r} {Describe(cells, $"D{r}")}, expected {line.Quantity}";
            else if (NumberAt(cells, $"E{r}") is not { } price || Math.Abs(price - line.UnitPrice) > 1e-9)
                problem = $"E{r} {Describe(cells, $"E{r}")}, expected {line.UnitPrice}";
            else if (!string.Equals(cells.GetValueOrDefault($"F{r}").Formula?.TrimStart('='),
                         TemplateData.AmountFormula(r), StringComparison.OrdinalIgnoreCase))
                problem = $"F{r} formula '{cells.GetValueOrDefault($"F{r}").Formula ?? "<none>"}', expected '{TemplateData.AmountFormula(r)}'";

            if (problem is null) continue;
            bad++;
            first ??= problem;
        }
        return (bad, first);
    }

    /// <summary>One cell as the file stores it, with a shared string already resolved.</summary>
    private readonly record struct SavedCell(string? Text, double? Number, string? Formula, int Style);

    private static Dictionary<string, SavedCell> ReadCells(XDocument sheet, List<string> sharedStrings)
    {
        var result = new Dictionary<string, SavedCell>(StringComparer.Ordinal);
        foreach (var c in sheet.Root!.Element(Main + "sheetData")!.Descendants(Main + "c"))
        {
            if ((string?)c.Attribute("r") is not { } reference) continue;

            var type = (string?)c.Attribute("t");
            var raw = c.Element(Main + "v")?.Value;
            string? text = type switch
            {
                "s" when int.TryParse(raw, CultureInfo.InvariantCulture, out var i) && i < sharedStrings.Count => sharedStrings[i],
                "inlineStr" => string.Concat(c.Element(Main + "is")?.Descendants(Main + "t").Select(t => t.Value) ?? []),
                "str" => raw,
                _ => null,
            };
            double? number = type is null or "n"
                             && double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)
                ? d
                : null;
            var style = int.TryParse((string?)c.Attribute("s"), CultureInfo.InvariantCulture, out var s) ? s : 0;

            result[reference] = new SavedCell(text, number, c.Element(Main + "f")?.Value, style);
        }
        return result;
    }

    private static string? Text(Dictionary<string, SavedCell> cells, string reference) =>
        cells.TryGetValue(reference, out var cell) ? cell.Text : null;

    private static double? NumberAt(Dictionary<string, SavedCell> cells, string reference) =>
        cells.TryGetValue(reference, out var cell) ? cell.Number : null;

    private static string Describe(Dictionary<string, SavedCell> cells, string reference) =>
        !cells.TryGetValue(reference, out var cell)
            ? "absent"
            : cell.Text is { } text
                ? $"'{text}'"
                : cell.Number is { } number
                    ? number.ToString("R", CultureInfo.InvariantCulture)
                    : cell.Formula is { } formula ? $"={formula}" : "empty";

    /// <summary>The number format code a cell's <c>s</c> index resolves to, or null for General.</summary>
    private static string? NumberFormatOf(XDocument? styles, int styleIndex)
    {
        var root = styles?.Root;
        var xf = root?.Element(Main + "cellXfs")?.Elements(Main + "xf").ElementAtOrDefault(styleIndex);
        if (xf is null || !int.TryParse((string?)xf.Attribute("numFmtId"), CultureInfo.InvariantCulture, out var id) || id == 0)
            return null;

        var custom = root!.Element(Main + "numFmts")?.Elements(Main + "numFmt")
            .FirstOrDefault(f => (string?)f.Attribute("numFmtId") == id.ToString(CultureInfo.InvariantCulture));
        if (custom is not null) return (string?)custom.Attribute("formatCode");

        // Built-in formats: 14–22 and 45–47 are dates and times; the rest that matter here are numeric.
        return id is >= 14 and <= 22 or >= 45 and <= 47 ? $"built-in date {id}" : $"built-in {id}";
    }

    private static bool IsDateFormat(string? format) =>
        format is not null && (format.StartsWith("built-in date", StringComparison.Ordinal)
                               || format.Contains('y', StringComparison.OrdinalIgnoreCase)
                               || format.Contains('d', StringComparison.OrdinalIgnoreCase));

    private static bool IsCurrencyFormat(string? format) =>
        format is not null && (format.Contains('$') || format.Contains("0.00", StringComparison.Ordinal));

    /// <summary>
    /// True when one of the saved ranges still covers the template's: same top-left, at least as
    /// far down. A library that inserts sheet rows to grow the table (EPPlus) also grows ranges
    /// that span the insertion point — exactly what Excel does on an insert, and not a loss.
    /// </summary>
    private static bool Covers(IEnumerable<string?> saved, string expected)
    {
        var (start, end) = Bounds(expected);
        return saved.OfType<string>()
            .SelectMany(s => s.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Select(Bounds)
            .Any(b => b.Start == start && b.End.Column == end.Column && b.End.Row >= end.Row);

        static ((int Row, int Column) Start, (int Row, int Column) End) Bounds(string range) =>
            range.Split(':') is [var from, var to]
                ? (TemplateData.ParseCell(from), TemplateData.ParseCell(to))
                : (TemplateData.ParseCell(range), TemplateData.ParseCell(range));
    }

    /// <summary>
    /// Schema errors the OpenXML SDK's validator finds, as one-line descriptions. Validated against
    /// the newest schema set the SDK knows, since the template is a current Excel's output.
    /// </summary>
    private static List<string> Validate(Stream package)
    {
        using var doc = SpreadsheetDocument.Open(package, isEditable: false);
        return new OpenXmlValidator(FileFormatVersions.Microsoft365)
            .Validate(doc)
            .Select(e => $"{e.Part?.Uri} {e.Path?.XPath}: {e.Description}")
            .ToList();
    }

    /// <summary>
    /// Just enough OPC to walk the package: resolve a part's relationships by type or id and load
    /// the targets. Relative targets are resolved against the source part's folder.
    /// </summary>
    private sealed class SavedPackage(ZipArchive zip)
    {
        public XDocument? Load(string? partPath)
        {
            if (partPath is null || zip.GetEntry(partPath) is not { } entry) return null;
            using var stream = entry.Open();
            return XDocument.Load(stream);
        }

        public string Target(string sourcePart, string relsPath, string type) =>
            Relationships(sourcePart, relsPath).First(r => r.Type == type).Target;

        public string? TargetOfType(string sourcePart, string type) =>
            TargetsOfType(sourcePart, type).FirstOrDefault();

        public IEnumerable<string> TargetsOfType(string sourcePart, string type) =>
            Relationships(sourcePart, RelsPathOf(sourcePart)).Where(r => r.Type == type).Select(r => r.Target);

        public string TargetById(string sourcePart, string id) =>
            Relationships(sourcePart, RelsPathOf(sourcePart)).First(r => r.Id == id).Target;

        private static string RelsPathOf(string part)
        {
            var slash = part.LastIndexOf('/');
            return slash < 0 ? $"_rels/{part}.rels" : $"{part[..slash]}/_rels/{part[(slash + 1)..]}.rels";
        }

        private IEnumerable<(string Id, string Type, string Target)> Relationships(string sourcePart, string relsPath)
        {
            if (Load(relsPath) is not { } rels) yield break;

            var folder = sourcePart.LastIndexOf('/') is var slash and >= 0 ? sourcePart[..slash] : string.Empty;
            foreach (var r in rels.Root!.Elements(Rel + "Relationship"))
            {
                if ((string?)r.Attribute("TargetMode") == "External") continue;
                var target = (string)r.Attribute("Target")!;
                yield return ((string)r.Attribute("Id")!, (string)r.Attribute("Type")!, Resolve(folder, target));
            }
        }

        private static string Resolve(string folder, string target)
        {
            if (target.StartsWith('/')) return target[1..];

            var parts = (folder.Length == 0 ? [] : folder.Split('/')).ToList();
            foreach (var segment in target.Split('/'))
            {
                if (segment == "..") parts.RemoveAt(parts.Count - 1);
                else if (segment != ".") parts.Add(segment);
            }
            return string.Join('/', parts);
        }
    }
}
