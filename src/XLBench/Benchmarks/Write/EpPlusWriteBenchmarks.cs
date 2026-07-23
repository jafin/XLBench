using BenchmarkDotNet.Attributes;
using OfficeOpenXml;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.Write;

public class EpPlusWriteBenchmarks
{
    [GlobalSetup]
    public void Setup() => EpPlusLicense.Ensure();

    [Benchmark]
    public void CreateAndSave()
    {
        using var pkg = new ExcelPackage();
        var ws = pkg.Workbook.Worksheets.Add("Data");

        ws.Cells[1, 1].Value = "Name";
        ws.Cells[1, 2].Value = "Amount";
        ws.Cells[1, 3].Value = "Date";

        for (var i = 0; i < TestData.WriteRowCount; i++)
        {
            var row = i + 2;
            ws.Cells[row, 1].Value = TestData.Strings[i];
            ws.Cells[row, 2].Value = TestData.Numbers[i];
            ws.Cells[row, 3].Value = TestData.Dates[i];
            ws.Cells[row, 3].Style.Numberformat.Format = "yyyy-mm-dd";
        }

        var sumRow = TestData.WriteRowCount + 2;
        ws.Cells[sumRow, 1].Value = "Total";
        ws.Cells[sumRow, 2].Formula = $"SUM(B2:B{TestData.WriteRowCount + 1})";

        using var ms = new MemoryStream();
        pkg.SaveAs(ms);
    }
}
