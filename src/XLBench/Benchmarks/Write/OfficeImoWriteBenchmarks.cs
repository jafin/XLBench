using BenchmarkDotNet.Attributes;
using OfficeIMO.Excel;
using XLBench.Data;

namespace XLBench.Benchmarks.Write;

/// <summary>
/// OfficeIMO addresses cells through the sheet rather than through a cell object, so each write
/// is one <c>ExcelSheet.CellValue(row, column, value)</c> call and the total row takes a real
/// <c>SUM</c> formula via <c>CellFormula</c>.
/// </summary>
public class OfficeImoWriteBenchmarks
{
    [Benchmark]
    public void CreateAndSave()
    {
        using var doc = ExcelDocument.Create();
        var ws = doc.AddSheet("Data");

        ws.Cell(1, 1).SetValue("Name");
        ws.Cell(1, 2).SetValue("Amount");
        ws.Cell(1, 3).SetValue("Date");

        for (var i = 0; i < TestData.WriteRowCount; i++)
        {
            var row = i + 2;
            ws.Cell(row, 1).SetValue(TestData.Strings[i]);
            ws.Cell(row, 2).SetValue(TestData.Numbers[i]);
            ws.Cell(row, 3).SetValue(TestData.Dates[i]);
        }

        var sumRow = TestData.WriteRowCount + 2;
        ws.Cell(sumRow, 1).SetValue("Total");
        ws.Cell(sumRow, 2).SetFormula($"SUM(B2:B{TestData.WriteRowCount + 1})");

        using var ms = new MemoryStream();
        doc.Save(ms);
    }
}
