using BenchmarkDotNet.Attributes;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using XLBench.Data;

namespace XLBench.Benchmarks.Read;

public class NpoiReadBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup() => _bytes = TestData.ReadXlsx;

    [Benchmark]
    public void OpenWorkbook()
    {
        using var wb = new XSSFWorkbook(new MemoryStream(_bytes));
    }

    [Benchmark]
    public long OpenAndReadAll()
    {
        using var wb = new XSSFWorkbook(new MemoryStream(_bytes));
        var sheet = wb.GetSheetAt(0);

        // Iterate rows and their populated cells rather than random GetRow/GetCell access.
        long checksum = 0;
        foreach (IRow row in sheet)
        {
            foreach (var cell in row.Cells)
                checksum += (cell.ToString() ?? string.Empty).Length;
        }
        return checksum;
    }
}
