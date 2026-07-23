using BenchmarkDotNet.Attributes;
using MiniExcelLibs;
using XLBench.Data;

namespace XLBench.Benchmarks.Read;

/// <summary>
/// MiniExcel is a streaming, row-at-a-time reader with no eager workbook object model, so
/// only <see cref="OpenAndReadAll"/> is benchmarked. Each row is an
/// <see cref="IDictionary{TKey,TValue}"/> keyed by column.
/// </summary>
public class MiniExcelReadBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup() => _bytes = TestData.ReadXlsx;

    [Benchmark]
    public long OpenAndReadAll()
    {
        using var stream = new MemoryStream(_bytes);

        long checksum = 0;
        foreach (IDictionary<string, object?> row in stream.Query(useHeaderRow: false, excelType: ExcelType.XLSX))
        foreach (var value in row.Values)
            checksum += (value?.ToString() ?? string.Empty).Length;
        return checksum;
    }
}
