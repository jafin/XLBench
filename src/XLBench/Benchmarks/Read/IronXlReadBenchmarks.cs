using BenchmarkDotNet.Attributes;
using IronXL;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.Read;

public class IronXlReadBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        IronXlLicense.Ensure();
        _bytes = TestData.ReadXlsx;
    }

    // WorkBook is not IDisposable — unlike every other eager-model library here, there is no
    // deterministic release of the loaded workbook; it is left to the GC.
    [Benchmark]
    public void OpenWorkbook() => _ = WorkBook.Load(new MemoryStream(_bytes));

    [Benchmark]
    public long OpenAndReadAll()
    {
        var wb = WorkBook.Load(new MemoryStream(_bytes));
        var ws = wb.DefaultWorkSheet;

        // FilledCells is IronXL's populated-cell enumeration — the equivalent of ClosedXML's
        // CellsUsed(), and the idiomatic alternative to random GetCellAt(row, col) access.
        long checksum = 0;
        foreach (var cell in ws.FilledCells)
            checksum += (cell.StringValue ?? string.Empty).Length;
        return checksum;
    }
}
