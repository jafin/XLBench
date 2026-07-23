using BenchmarkDotNet.Attributes;
using ClosedXML.Excel;
using XLBench.Data;

namespace XLBench.Benchmarks.Read;

public class ClosedXmlReadBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup() => _bytes = TestData.ReadXlsx;

    [Benchmark]
    public void OpenWorkbook()
    {
        using var wb = new XLWorkbook(new MemoryStream(_bytes));
    }

    [Benchmark]
    public long OpenAndReadAll()
    {
        using var wb = new XLWorkbook(new MemoryStream(_bytes));
        var ws = wb.Worksheets.First();

        long checksum = 0;
        for (var row = 1; row <= TestData.ReadRowCount; row++)
        for (var col = 1; col <= TestData.ReadColCount; col++)
            checksum += ws.Cell(row, col).GetValue<string>().Length;
        return checksum;
    }
}
