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

        // Idiomatic iteration over populated cells (random Cell(row,col) access is
        // pathologically slow in ClosedXML and not how sheets are read in practice).
        long checksum = 0;
        foreach (var cell in ws.CellsUsed())
            checksum += cell.GetString().Length;
        return checksum;
    }
}
