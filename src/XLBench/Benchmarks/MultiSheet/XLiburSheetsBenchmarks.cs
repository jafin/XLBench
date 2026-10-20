using BenchmarkDotNet.Attributes;
using XLBench.Data;
using XLibur.Excel;

namespace XLBench.Benchmarks.MultiSheet;

/// <summary>
/// XLibur multi-sheet read. The API is ClosedXML-shaped, so this mirrors
/// <see cref="ClosedXmlSheetsBenchmarks"/> line for line: <c>Worksheets</c>, <c>CellsUsed()</c>,
/// and <c>GetString()</c> for the plain text of a text or rich-text cell.
/// </summary>
public class XLiburSheetsBenchmarks
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
        using var wb = new XLWorkbook(new MemoryStream(bytes));

        long cells = 0;
        long textChars = 0;
        foreach (var ws in wb.Worksheets)
        foreach (var cell in ws.CellsUsed())
        {
            cells++;
            if (cell.DataType == XLDataType.Text)
                textChars += cell.GetString().Length;
        }
        return SheetsData.Checksum(cells, textChars);
    }
}
