using BenchmarkDotNet.Attributes;
using OfficeOpenXml;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.Read;

public class EpPlusReadBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        EpPlusLicense.Ensure();
        _bytes = TestData.ReadXlsx;
    }

    [Benchmark]
    public void OpenWorkbook()
    {
        using var pkg = new ExcelPackage(new MemoryStream(_bytes));
        _ = pkg.Workbook.Worksheets.Count;
    }

    [Benchmark]
    public long OpenAndReadAll()
    {
        using var pkg = new ExcelPackage(new MemoryStream(_bytes));
        var ws = pkg.Workbook.Worksheets.First();

        long checksum = 0;
        for (var row = 1; row <= TestData.ReadRowCount; row++)
        for (var col = 1; col <= TestData.ReadColCount; col++)
            checksum += (ws.Cells[row, col].GetValue<string>() ?? string.Empty).Length;
        return checksum;
    }
}
