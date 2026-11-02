using BenchmarkDotNet.Attributes;
using IronXL;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.Write;

public class IronXlWriteBenchmarks
{
    [GlobalSetup]
    public void Setup() => IronXlLicense.Ensure();

    [Benchmark]
    public void CreateAndSave()
    {
        var wb = WorkBook.Create(ExcelFileFormat.XLSX);
        var ws = wb.CreateWorkSheet("Data");

        // IronXL's indexed setter is 0-based; the shared layout constants are 1-based.
        ws.SetCellValue(0, 0, "Name");
        ws.SetCellValue(0, 1, "Amount");
        ws.SetCellValue(0, 2, "Date");

        for (var i = 0; i < TestData.WriteRowCount; i++)
        {
            var row = i + 1;
            ws.SetCellValue(row, 0, TestData.Strings[i]);
            ws.SetCellValue(row, 1, TestData.Numbers[i]);
            ws.SetCellValue(row, 2, TestData.Dates[i]);
        }

        var sumRow = TestData.WriteRowCount + 2;
        ws.SetCellValue(sumRow - 1, 0, "Total");
        ws[$"B{sumRow}"].Formula = $"SUM(B2:B{TestData.WriteRowCount + 1})";

        // No SaveAs(Stream) overload — ToStream() materializes the whole workbook into a
        // MemoryStream IronXL allocates itself, so the buffer is part of what is measured here.
        using var ms = wb.ToStream();
    }
}
