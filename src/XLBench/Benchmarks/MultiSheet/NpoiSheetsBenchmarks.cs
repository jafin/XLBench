using BenchmarkDotNet.Attributes;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using XLBench.Data;

namespace XLBench.Benchmarks.MultiSheet;

/// <summary>
/// NPOI multi-sheet read: <c>XSSFWorkbook</c> enumerates its sheets, and each sheet its rows and
/// their populated cells, as in <see cref="Read.NpoiReadBenchmarks"/>.
///
/// <para>Text cells are <c>CellType.String</c>. <c>StringCellValue</c> is the plain text of a
/// rich-text cell; the runs are on <c>RichStringCellValue</c>.</para>
/// </summary>
public class NpoiSheetsBenchmarks
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
        using var wb = new XSSFWorkbook(new MemoryStream(bytes));

        long cells = 0;
        long textChars = 0;
        foreach (ISheet sheet in wb)
        foreach (IRow row in sheet)
        foreach (var cell in row.Cells)
        {
            cells++;
            if (cell.CellType == CellType.String)
                textChars += cell.StringCellValue.Length;
        }
        return SheetsData.Checksum(cells, textChars);
    }
}
