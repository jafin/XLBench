using System.Diagnostics;
using System.IO.Compression;
using System.Xml.Linq;
using XLBench.Benchmarks.MultiSheet;
using XLBench.Libraries;

namespace XLBench.Data;

/// <summary>
/// Runs every library's <c>OpenAndReadAllSheets</c> once, unmeasured, and checks the checksum it
/// returns against the one computed from the generator's data (<c>dotnet run -- sheets</c>).
///
/// <para>A read writes nothing, so unlike the edit and insert commands there is no artifact to
/// save. What can go wrong is a library that misses a sheet, skips cells, or returns rich text
/// with its formatting markup still in it; the cell count and the text length catch each of
/// those. It also prints the workbook's shared-string and style counts, read straight from the
/// package, so the scenario's claims about its size can be checked.</para>
///
/// <para>The elapsed time it prints is a single cold pass — indicative only, never a benchmark
/// result.</para>
/// </summary>
public static class SheetsArtifacts
{
    private static readonly XNamespace Main =
        "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

    public static void WriteAll()
    {
        SheetsData.EnsureLoaded();

        var expected = SheetsData.ExpectedChecksum;
        var (expectedCells, expectedChars) = SheetsData.Split(expected);

        Console.WriteLine(
            $"[XLBench] Sheets scenario: {SheetsData.SheetCount} sheets x {SheetsData.RowsPerSheet + 1:N0} rows x " +
            $"{SheetsData.ColCount} columns, {SheetsData.SourceXlsx.Length / 1024:N0} KB.");
        Console.WriteLine($"[XLBench] {DescribePackage(SheetsData.SourceXlsx)}");
        Console.WriteLine($"[XLBench] Expected {expectedCells:N0} cells and {expectedChars:N0} characters of text.");

        // One failing library must not stop the others; the exception text is the useful output.
        (string Library, Func<byte[], long> Read)[] readers =
        [
            ("ClosedXML", ClosedXmlSheetsBenchmarks.Read),
            ("EPPlus", bytes =>
            {
                EpPlusLicense.Ensure();
                return EpPlusSheetsBenchmarks.Read(bytes);
            }),
            ("NPOI", NpoiSheetsBenchmarks.Read),
            ("OpenXML SDK", OpenXmlSheetsBenchmarks.Read),
            ("MiniExcel", MiniExcelSheetsBenchmarks.Read),
            ("Sylvan", SylvanSheetsBenchmarks.Read),
            ("XLibur", XLiburSheetsBenchmarks.Read),
            ("OfficeIMO", OfficeImoSheetsBenchmarks.Read),
            ("Telerik", TelerikSheetsBenchmarks.Read),
            ("IronXL", IronXlSheetsBenchmarks.Read),
        ];

        // IronXL throws rather than watermarking when unlicensed (see IronXlLicense).
        if (!IronXlLicense.KeyAvailable)
        {
            readers = [.. readers.Where(r => r.Library != "IronXL")];
            Console.WriteLine("[XLBench] Skipping IronXL — XLBENCH_IRONXL_KEY not set (see README).");
        }

        // An unlicensed Telerik is excluded from the benchmarks (see TelerikLicense), so it is
        // left out here too, to keep the two lists the same.
        if (!TelerikLicense.IsLicensed)
        {
            readers = [.. readers.Where(r => r.Library != "Telerik")];
            Console.WriteLine("[XLBench] Skipping Telerik — no licence found (see README).");
        }

        var ok = 0;
        var wrong = 0;
        var failed = 0;

        foreach (var (library, read) in readers)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                var actual = read(SheetsData.SourceXlsx);
                sw.Stop();

                var (cells, chars) = SheetsData.Split(actual);
                var matches = actual == expected;
                if (matches) ok++;
                else wrong++;

                Console.WriteLine(
                    $"[XLBench] {library}: {(matches ? "OK" : "MISMATCH")} {cells:N0} cells, {chars:N0} characters" +
                    $" — {sw.ElapsedMilliseconds:N0} ms (single cold pass, not a benchmark result).");

                if (!matches)
                {
                    Console.WriteLine(
                        $"[XLBench]   expected {expectedCells:N0} cells ({cells - expectedCells:+#,0;-#,0;0})" +
                        $" and {expectedChars:N0} characters ({chars - expectedChars:+#,0;-#,0;0}).");
                }
            }
#pragma warning disable S2221 // A broken library must not prevent the others from being checked
            catch (Exception ex)
#pragma warning restore S2221
            {
                failed++;
                Console.WriteLine($"[XLBench] {library} failed: {ex}");
            }
        }

        Console.WriteLine($"[XLBench] {ok} of {readers.Length} library read(s) matched."
            + (wrong > 0 ? $" {wrong} returned a wrong checksum." : string.Empty)
            + (failed > 0 ? $" {failed} failed." : string.Empty));
    }

    /// <summary>
    /// Counts the parts that make this workbook expensive, straight from the package XML: the
    /// shared-string table and how much of it is rich text, and the cell-format table.
    /// </summary>
    private static string DescribePackage(byte[] xlsx)
    {
        using var zip = new ZipArchive(new MemoryStream(xlsx), ZipArchiveMode.Read);

        var sst = Load(zip, "xl/sharedStrings.xml").Root!;
        var items = sst.Elements(Main + "si").ToList();
        var rich = items.Count(si => si.Element(Main + "r") is not null);

        var styles = Load(zip, "xl/styles.xml").Root!;
        int Count(string table, string item) => styles.Element(Main + table)?.Elements(Main + item).Count() ?? 0;

        // How the text cells point at their strings. Inline strings would bypass the shared table
        // and make the scenario cheaper than it claims to be.
        var sheets = 0;
        var shared = 0;
        var inline = 0;
        foreach (var entry in zip.Entries.Where(e => e.FullName.StartsWith("xl/worksheets/sheet", StringComparison.OrdinalIgnoreCase)))
        {
            sheets++;
            using var stream = entry.Open();
            foreach (var c in XDocument.Load(stream).Descendants(Main + "c"))
            {
                switch ((string?)c.Attribute("t"))
                {
                    case "s": shared++; break;
                    case "inlineStr": inline++; break;
                }
            }
        }

        return $"Package: {sheets} worksheet parts; {shared:N0} shared-string cells and {inline} inline strings;" +
               $" {items.Count:N0} unique shared strings, {rich:N0} of them rich text;" +
               $" cellXfs={Count("cellXfs", "xf"):N0}, fonts={Count("fonts", "font")}," +
               $" fills={Count("fills", "fill")}, borders={Count("borders", "border")}," +
               $" numFmts={Count("numFmts", "numFmt")}.";
    }

    private static XDocument Load(ZipArchive zip, string name)
    {
        var entry = zip.GetEntry(name) ?? throw new InvalidOperationException($"Package has no '{name}'.");
        using var stream = entry.Open();
        return XDocument.Load(stream);
    }
}
