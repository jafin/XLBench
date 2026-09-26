using BenchmarkDotNet.Attributes;
using Sylvan.Data;
using Sylvan.Data.Excel;
using XLBench.Data;

namespace XLBench.Benchmarks.Write;

/// <summary>
/// Sylvan.Data.Excel writes a whole <see cref="System.Data.Common.DbDataReader"/> to a worksheet
/// rather than exposing a cell model. The rows are streamed through Sylvan.Data's
/// <c>AsDataReader()</c> adapter, and the property names become the header row. Note: the writer
/// has no formula support, so, as with MiniExcel, the total row is written as a pre-computed value
/// rather than a SUM() formula.
/// </summary>
public class SylvanWriteBenchmarks
{
    [Benchmark]
    public void CreateAndSave()
    {
        using var ms = new MemoryStream();
        using (var writer = ExcelDataWriter.Create(ms, ExcelWorkbookType.ExcelXml))
            writer.Write(EnumerateRows().AsDataReader(), "Data");
    }

    private sealed record Row(string Name, double Amount, DateTime? Date);

    private static IEnumerable<Row> EnumerateRows()
    {
        for (var i = 0; i < TestData.WriteRowCount; i++)
            yield return new Row(TestData.Strings[i], TestData.Numbers[i], TestData.Dates[i]);

        double total = 0;
        for (var i = 0; i < TestData.WriteRowCount; i++)
            total += TestData.Numbers[i];

        yield return new Row("Total", total, null);
    }
}
