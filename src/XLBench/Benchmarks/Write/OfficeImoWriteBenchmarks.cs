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
        var ws = doc.AddWorksheet("Data");

        ws.CellValue(1, 1, "Name");
        ws.CellValue(1, 2, "Amount");
        ws.CellValue(1, 3, "Date");

        for (var i = 0; i < TestData.WriteRowCount; i++)
        {
            var row = i + 2;
            ws.CellValue(row, 1, TestData.Strings[i]);
            ws.CellValue(row, 2, TestData.Numbers[i]);
            ws.CellValue(row, 3, TestData.Dates[i]);
        }

        var sumRow = TestData.WriteRowCount + 2;
        ws.CellValue(sumRow, 1, "Total");
        ws.CellFormula(sumRow, 2, $"SUM(B2:B{TestData.WriteRowCount + 1})");

        using var ms = new MemoryStream();
        doc.Save(ms);
    }
}
