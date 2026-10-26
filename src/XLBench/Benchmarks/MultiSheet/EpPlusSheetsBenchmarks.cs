using BenchmarkDotNet.Attributes;
using OfficeOpenXml;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.MultiSheet;

/// <summary>
/// EPPlus multi-sheet read: every sheet through <c>Workbook.Worksheets</c>, every populated cell
/// through <c>ws.Cells</c>, as in <see cref="Read.EpPlusReadBenchmarks"/>.
///
/// <para>A text cell's <c>Value</c> is a <see cref="string"/>; for a rich-text cell it is the
/// runs joined into plain text, with the runs themselves behind <c>RichText</c>.</para>
/// </summary>
public class EpPlusSheetsBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        EpPlusLicense.Ensure();
        SheetsData.EnsureLoaded();
        _bytes = SheetsData.SourceXlsx;
    }

    [Benchmark]
    public long OpenAndReadAllSheets() => Read(_bytes);

    /// <inheritdoc cref="ClosedXmlSheetsBenchmarks.Read"/>
    internal static long Read(byte[] bytes)
    {
        using var pkg = new ExcelPackage(new MemoryStream(bytes));

        long cells = 0;
        long textChars = 0;
        foreach (var ws in pkg.Workbook.Worksheets)
        foreach (var cell in ws.Cells)
        {
            cells++;
            if (cell.Value is string text)
                textChars += text.Length;
        }
        return SheetsData.Checksum(cells, textChars);
    }
}
