using BenchmarkDotNet.Attributes;
using Sylvan.Data.Excel;
using XLBench.Data;

namespace XLBench.Benchmarks.Read;

/// <summary>
/// Sylvan.Data.Excel is a forward-only <see cref="System.Data.Common.DbDataReader"/> over the
/// worksheet, with no workbook object model, so only <see cref="OpenAndReadAll"/> is benchmarked.
/// <c>ExcelSchema.NoHeaders</c> makes it return the header row as data, so every populated cell is
/// read, as in the other libraries' benchmarks.
/// </summary>
public class SylvanReadBenchmarks
{
    private static readonly ExcelDataReaderOptions Options = new() { Schema = ExcelSchema.NoHeaders };

    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup() => _bytes = TestData.ReadXlsx;

    [Benchmark]
    public long OpenAndReadAll()
    {
        using var reader = ExcelDataReader.Create(new MemoryStream(_bytes), ExcelWorkbookType.ExcelXml, Options);

        long checksum = 0;
        while (reader.Read())
        {
            for (var i = 0; i < reader.RowFieldCount; i++)
                checksum += reader.GetString(i).Length;
        }
        return checksum;
    }
}
