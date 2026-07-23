using BenchmarkDotNet.Attributes;
using MiniExcelLibs;
using XLBench.Data;

namespace XLBench.Benchmarks.Write;

/// <summary>
/// MiniExcel writes from a data source rather than a cell model. Rows are streamed as
/// dictionaries so MiniExcel serializes them lazily. Note: MiniExcel has no formula engine,
/// so the total row is written as a pre-computed value rather than a SUM() formula.
/// </summary>
public class MiniExcelWriteBenchmarks
{
    [Benchmark]
    public void CreateAndSave()
    {
        using var ms = new MemoryStream();
        ms.SaveAs(EnumerateRows(), printHeader: true, excelType: ExcelType.XLSX);
    }

    private static IEnumerable<Dictionary<string, object>> EnumerateRows()
    {
        for (var i = 0; i < TestData.WriteRowCount; i++)
        {
            yield return new Dictionary<string, object>
            {
                ["Name"] = TestData.Strings[i],
                ["Amount"] = TestData.Numbers[i],
                ["Date"] = TestData.Dates[i],
            };
        }

        double total = 0;
        for (var i = 0; i < TestData.WriteRowCount; i++)
            total += TestData.Numbers[i];

        yield return new Dictionary<string, object>
        {
            ["Name"] = "Total",
            ["Amount"] = total,
            ["Date"] = string.Empty,
        };
    }
}
