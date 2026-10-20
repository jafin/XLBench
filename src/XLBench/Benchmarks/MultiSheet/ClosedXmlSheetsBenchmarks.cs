using BenchmarkDotNet.Attributes;
using ClosedXML.Excel;
using XLBench.Data;

namespace XLBench.Benchmarks.MultiSheet;

/// <summary>
/// ClosedXML multi-sheet read: every sheet through <c>Worksheets</c>, every populated cell through
/// <c>CellsUsed()</c>, as in <see cref="Read.ClosedXmlReadBenchmarks"/>.
///
/// <para>Text cells are recognised by <c>DataType == XLDataType.Text</c>. A rich-text cell is a
/// text cell too, and <c>GetString()</c> returns its runs joined into plain text, so no extra
/// call is needed for it.</para>
/// </summary>
public class ClosedXmlSheetsBenchmarks
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

    /// <summary>
    /// Reads every populated cell of every sheet and returns <see cref="SheetsData.Checksum"/>,
    /// so the work cannot be optimized away and <see cref="SheetsArtifacts"/> can check it.
    /// </summary>
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
