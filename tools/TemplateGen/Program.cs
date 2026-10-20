using System.Runtime.InteropServices;

// TemplateGen — authors the Template scenario's workbook in Excel itself.
//
// The scenario's point is filling a file a person built in Excel, so every byte of
// src/XLBench/Data/sales_template.xlsx has to come from Excel's own writer: its theme, its
// styles part, its table and chart XML, its x14 extensions. A library-built "template" would
// only measure how well libraries read each other's output.
//
// Usage (from the repo root; needs Windows and desktop Excel):
//
//   dotnet run --project tools/TemplateGen
//   dotnet run --project tools/TemplateGen -- <out.xlsx>
//
// Excel is driven through late-bound IDispatch (`dynamic`). The typed interop interfaces fail
// on some installs with TYPE_E_ELEMENTNOTFOUND, and late binding needs no interop assembly.
//
// Layout (sheet "Report"):
//
//        A             B          C          D          E           F        G   H          I          J
//   1 | Monthly Sales Report (merged A1:F1, named ReportTitle)          |   |          |          |
//   2 | Report date | 2026-01-31 (ReportDate)                           |   |          |          |
//   3 | Region      | All regions (Region)                              |   |          |          |
//   5 | Date        | Product  | Category | Quantity | UnitPrice | Amount |   | Category | Amount   | Share
//   6 | (one empty placeholder row — the Sales table, A5:F6)               |   | Hardware | =SUMIFS(Sales[Amount],Sales[Category],H6) | =I6/I11
//  ...                                                                        |   | ...      |          |
//  11 |                                                                       |   | Total    | =SUBTOTAL(109,Sales[Amount])
//  13 |                                                                       |   | clustered column chart of H5:I10
//
// The chart plots the fixed summary block rather than the table, so it never needs its range
// to grow: filling the table changes what SUMIFS returns, not what the chart points at.

const int xlOpenXMLWorkbook = 51;
const int xlSrcRange = 1;
const int xlYes = 1;
const int xlValidateList = 3;
const int xlValidAlertStop = 1;
const int xlBetween = 1;
const int xlCellValue = 1;
const int xlGreater = 5;
const int xlColumnClustered = 51;
const int xlColumns = 2;
const int xlCenter = -4108;

var outPath = Path.GetFullPath(args.Length > 0
    ? args[0]
    : Path.Combine("src", "XLBench", "Data", "sales_template.xlsx"));

string[] categories = ["Hardware", "Software", "Services", "Accessories", "Training"];

var excelType = Type.GetTypeFromProgID("Excel.Application")
                ?? throw new InvalidOperationException("Excel is not installed (no Excel.Application ProgID).");

dynamic excel = Activator.CreateInstance(excelType)!;
var excelProcessId = ProcessIdOf((IntPtr)(int)excel.Hwnd);
dynamic? workbook = null;
try
{
    excel.Visible = false;
    excel.DisplayAlerts = false;
    excel.ScreenUpdating = false;

    workbook = excel.Workbooks.Add();
    dynamic sheet = workbook.Worksheets[1];
    sheet.Name = "Report";

    // Remove any extra default sheets so the template has exactly one.
    while ((int)workbook.Worksheets.Count > 1)
        workbook.Worksheets[2].Delete();

    // ---- Named cell styles -------------------------------------------------------------
    // Real templates carry their own styles; these land in styles.xml as cellStyleXfs with
    // matching <cellStyle> names, which is what a library has to round-trip.
    dynamic titleStyle = workbook.Styles.Add("Report Title");
    titleStyle.Font.Bold = true;
    titleStyle.Font.Size = 16;
    titleStyle.Font.Color = 0x7F3F1F; // BGR: dark blue
    titleStyle.HorizontalAlignment = xlCenter;

    dynamic dateStyle = workbook.Styles.Add("Report Date");
    dateStyle.NumberFormat = "yyyy-mm-dd";

    dynamic currencyStyle = workbook.Styles.Add("Report Currency");
    currencyStyle.NumberFormat = "$#,##0.00";

    dynamic percentStyle = workbook.Styles.Add("Report Percent");
    percentStyle.NumberFormat = "0.0%";

    // Whole-column formats first, then the cell-level styles on top: the template's author
    // formatted the data columns once, so new rows written below the table pick the format up
    // from <col style> — if the library honours it.
    sheet.Columns["A"].Style = "Report Date";
    sheet.Columns["D"].NumberFormat = "0";
    sheet.Columns["E"].Style = "Report Currency";
    sheet.Columns["F"].Style = "Report Currency";

    // ---- Header block ------------------------------------------------------------------
    dynamic title = sheet.Range["A1:F1"];
    title.Merge();
    title.Style = "Report Title";
    sheet.Range["A1"].Value2 = "Monthly Sales Report";
    sheet.Rows[1].RowHeight = 24;

    sheet.Range["A2"].Value2 = "Report date";
    sheet.Range["A2"].Style = "Normal";
    sheet.Range["B2"].Style = "Report Date";
    sheet.Range["B2"].Formula = "=DATE(2026,1,31)";
    sheet.Range["B2"].Value2 = sheet.Range["B2"].Value2; // freeze to a literal date serial
    sheet.Range["A3"].Value2 = "Region";
    sheet.Range["A3"].Style = "Normal";
    sheet.Range["B3"].Value2 = "All regions";
    sheet.Range["A2:A3"].Font.Bold = true;

    workbook.Names.Add("ReportTitle", "=Report!$A$1");
    workbook.Names.Add("ReportDate", "=Report!$B$2");
    workbook.Names.Add("Region", "=Report!$B$3");

    // ---- Sales table -------------------------------------------------------------------
    string[] headers = ["Date", "Product", "Category", "Quantity", "UnitPrice", "Amount"];
    for (var i = 0; i < headers.Length; i++)
    {
        sheet.Cells[5, i + 1].Value2 = headers[i];
        sheet.Cells[5, i + 1].Style = "Normal";
    }

    // One empty data row: Excel will not create a table without one, and an empty row is what
    // a fill-me template ships with.
    dynamic table = sheet.ListObjects.Add(xlSrcRange, sheet.Range["A5:F6"], Type.Missing, xlYes);
    table.Name = "Sales";
    table.TableStyle = "TableStyleMedium2";

    // ---- Data validation and conditional formatting ------------------------------------
    // Both cover far more rows than the table, the way a template author sets them up so the
    // rules are already in place wherever the data ends.
    dynamic validation = sheet.Range["C6:C5000"].Validation;
    validation.Delete();
    validation.Add(xlValidateList, xlValidAlertStop, xlBetween, string.Join(",", categories));
    validation.IgnoreBlank = true;
    validation.InCellDropdown = true;

    dynamic rule = sheet.Range["F6:F5000"].FormatConditions.Add(xlCellValue, xlGreater, "=1000");
    rule.Interior.Color = 0xCEEFC6; // BGR: light green
    rule.Font.Color = 0x006100;     // BGR: dark green

    // ---- Summary block and chart -------------------------------------------------------
    sheet.Range["H5"].Value2 = "Category";
    sheet.Range["I5"].Value2 = "Amount";
    sheet.Range["J5"].Value2 = "Share";
    sheet.Range["H5:J5"].Font.Bold = true;
    for (var i = 0; i < categories.Length; i++)
    {
        var row = 6 + i;
        sheet.Range[$"H{row}"].Value2 = categories[i];
        sheet.Range[$"I{row}"].Formula = $"=SUMIFS(Sales[Amount],Sales[Category],H{row})";
        sheet.Range[$"I{row}"].Style = "Report Currency";
        sheet.Range[$"J{row}"].Formula = $"=IF($I$11=0,0,I{row}/$I$11)";
        sheet.Range[$"J{row}"].Style = "Report Percent";
    }
    sheet.Range["H11"].Value2 = "Total";
    sheet.Range["H11"].Font.Bold = true;
    sheet.Range["I11"].Formula = "=SUBTOTAL(109,Sales[Amount])";
    sheet.Range["I11"].Style = "Report Currency";
    sheet.Range["I11"].Font.Bold = true;

    dynamic anchor = sheet.Range["H13"];
    dynamic shape = sheet.Shapes.AddChart2(-1, xlColumnClustered,
        (double)anchor.Left, (double)anchor.Top, 420d, 240d);
    shape.Name = "SalesByCategory";
    dynamic chart = shape.Chart;
    // Series in columns: one "Amount" series over the five categories. Left to guess, Excel
    // reads a 5×2 block as five single-point series.
    chart.SetSourceData(sheet.Range["H5:I10"], xlColumns);
    chart.HasTitle = true;
    chart.ChartTitle.Text = "Sales by category";

    // ---- Column widths and document properties -----------------------------------------
    double[] widths = [12, 18, 14, 10, 12, 14, 3, 14, 14, 9];
    for (var i = 0; i < widths.Length; i++)
        sheet.Columns[i + 1].ColumnWidth = widths[i];

    workbook.BuiltinDocumentProperties["Title"].Value = "XLBench sales report template";
    // Strips the author and the absolute save path Excel records in workbook.xml, so the
    // checked-in file says nothing about the machine that generated it.
    workbook.RemovePersonalInformation = true;
    sheet.Range["A6"].Select();

    Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
    if (File.Exists(outPath)) File.Delete(outPath);
    workbook.SaveAs(outPath, xlOpenXMLWorkbook);
    Console.WriteLine($"[TemplateGen] Saved {outPath} ({new FileInfo(outPath).Length:N0} bytes)");
    return 0;
}
finally
{
    // Always let Excel go, even on failure: a leaked hidden EXCEL.EXE holds file locks and
    // breaks the next run.
    if (workbook is not null)
    {
        try { workbook.Close(false); } catch (COMException) { /* already closing */ }
        Marshal.FinalReleaseComObject(workbook);
    }
    try { excel.Quit(); } catch (COMException) { /* already gone */ }
    Marshal.FinalReleaseComObject(excel);
    GC.Collect();
    GC.WaitForPendingFinalizers();

    // Every intermediate `dynamic` (Range, Font, Validation …) holds a reference of its own, and
    // the instance stays alive until all of them are finalized — which, for top-level locals, is
    // not before the process exits. Quit has already discarded the workbook, so ending the
    // instance this tool started (and only that one, by PID) loses nothing.
    if (excelProcessId != 0)
    {
        try
        {
            using var process = System.Diagnostics.Process.GetProcessById(excelProcessId);
            if (!process.WaitForExit(5_000)) process.Kill();
        }
        catch (ArgumentException) { /* already exited */ }
    }
}

static int ProcessIdOf(IntPtr hwnd)
{
    _ = GetWindowThreadProcessId(hwnd, out var pid);
    return (int)pid;
}

[DllImport("user32.dll")]
static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
