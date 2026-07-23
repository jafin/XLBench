using BenchmarkDotNet.Attributes;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using XLBench.Data;

namespace XLBench.Benchmarks.Write;

public class NpoiWriteBenchmarks
{
    [Benchmark]
    public void CreateAndSave()
    {
        using var wb = new XSSFWorkbook();
        var sheet = wb.CreateSheet("Data");

        var dateStyle = wb.CreateCellStyle();
        dateStyle.DataFormat = wb.CreateDataFormat().GetFormat("yyyy-mm-dd");

        var header = sheet.CreateRow(0);
        header.CreateCell(0).SetCellValue("Name");
        header.CreateCell(1).SetCellValue("Amount");
        header.CreateCell(2).SetCellValue("Date");

        for (var i = 0; i < TestData.WriteRowCount; i++)
        {
            var row = sheet.CreateRow(i + 1);
            row.CreateCell(0).SetCellValue(TestData.Strings[i]);
            row.CreateCell(1).SetCellValue(TestData.Numbers[i]);
            var dateCell = row.CreateCell(2);
            dateCell.SetCellValue(TestData.Dates[i]);
            dateCell.CellStyle = dateStyle;
        }

        var totalRow = sheet.CreateRow(TestData.WriteRowCount + 1);
        totalRow.CreateCell(0).SetCellValue("Total");
        totalRow.CreateCell(1).SetCellFormula($"SUM(B2:B{TestData.WriteRowCount + 1})");

        using var ms = new MemoryStream();
        wb.Write(ms, leaveOpen: true);
    }
}
