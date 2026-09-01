using BenchmarkDotNet.Attributes;
using Telerik.Documents.Spreadsheet.FormatProviders.OpenXml.Xlsx;
using Telerik.Documents.Spreadsheet.Model;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.Write;

/// <summary>
/// RadSpreadProcessing write: build the cell model, then hand it to the format provider.
///
/// Cell coordinates are 0-based here where the rest of the suite is 1-based, so every index is
/// one lower than the equivalent ClosedXML/EPPlus line. The sheet produced is the same.
/// </summary>
public class TelerikWriteBenchmarks
{
    [Benchmark]
    public void CreateAndSave()
    {
        using var workbook = new Workbook().WithoutHistory();
        var worksheet = workbook.Worksheets.Add();
        worksheet.Name = "Data";

        worksheet.Cells[0, 0].SetValue("Name");
        worksheet.Cells[0, 1].SetValue("Amount");
        worksheet.Cells[0, 2].SetValue("Date");

        for (var i = 0; i < TestData.WriteRowCount; i++)
        {
            var row = i + 1;
            worksheet.Cells[row, 0].SetValue(TestData.Strings[i]);
            worksheet.Cells[row, 1].SetValue(TestData.Numbers[i]);
            worksheet.Cells[row, 2].SetValue(TestData.Dates[i]);
        }

        var sumRow = TestData.WriteRowCount + 1;
        worksheet.Cells[sumRow, 0].SetValue("Total");
        worksheet.Cells[sumRow, 1].SetValueAsFormula($"=SUM(B2:B{TestData.WriteRowCount + 1})");

        using var ms = new MemoryStream();
        new XlsxFormatProvider().Export(workbook, ms, null);
    }
}
