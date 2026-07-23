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

        long checksum = 0;
        for (var r = 0; r < TestData.ReadRowCount; r++)
        {
            var row = sheet.GetRow(r);
            if (row is null) continue;
            for (var c = 0; c < TestData.ReadColCount; c++)
            {
                var cell = row.GetCell(c);
                if (cell is null) continue;
                checksum += (cell.ToString() ?? string.Empty).Length;
            }
        }
        return checksum;
    }
}
