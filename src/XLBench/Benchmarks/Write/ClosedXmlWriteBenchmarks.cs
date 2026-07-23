using BenchmarkDotNet.Attributes;
using ClosedXML.Excel;
using XLBench.Data;

namespace XLBench.Benchmarks.Write;

public class ClosedXmlWriteBenchmarks
{
    [Benchmark]
    public void CreateAndSave()
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Data");

        ws.Cell(1, 1).Value = "Name";
        ws.Cell(1, 2).Value = "Amount";
        ws.Cell(1, 3).Value = "Date";

        for (var i = 0; i < TestData.WriteRowCount; i++)
        {
            var row = i + 2;
            ws.Cell(row, 1).Value = TestData.Strings[i];
            ws.Cell(row, 2).Value = TestData.Numbers[i];
            ws.Cell(row, 3).Value = TestData.Dates[i];
        }

        var sumRow = TestData.WriteRowCount + 2;
        ws.Cell(sumRow, 1).Value = "Total";
        ws.Cell(sumRow, 2).FormulaA1 = $"SUM(B2:B{TestData.WriteRowCount + 1})";

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
    }
}
