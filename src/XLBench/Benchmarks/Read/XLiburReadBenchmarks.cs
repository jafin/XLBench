using BenchmarkDotNet.Attributes;
using XLBench.Data;
using XLibur.Excel;

namespace XLBench.Benchmarks.Read;

public class XLiburReadBenchmarks
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

        // Idiomatic iteration over populated cells (mirrors the ClosedXML benchmark).
        long checksum = 0;
        foreach (var cell in ws.CellsUsed())
            checksum += cell.GetString().Length;
        return checksum;
    }
}
