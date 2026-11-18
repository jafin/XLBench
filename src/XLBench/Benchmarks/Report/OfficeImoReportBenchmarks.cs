using BenchmarkDotNet.Attributes;
using OfficeIMO;
using OfficeIMO.Drawing;
using OfficeIMO.Excel;
using XLBench.Data;

namespace XLBench.Benchmarks.Report;

/// <summary>
/// OfficeIMO stock report: data + conditional formatting + line chart — full scenario support.
///
/// <para>Styling is per cell: there is no row style, so the header is bolded cell by cell, and the
/// number formats are applied to each cell as it is written, as XLibur and ClosedXML do.</para>
///
/// <para>The convenience conditional-format helpers (<c>AddConditionalFormulaRule</c>) take a fill
/// colour only. The scenario also sets a font colour, so the two expression rules go through
/// <c>AddConditionalFormattingRule</c> with an <c>ExcelConditionalFormattingInfo</c>, which carries
/// both into one differential format.</para>
///
/// <para>The chart is bound to the sheet block through <c>ExcelChartDataRange</c>: categories in
/// the week-ending column, one series per symbol column, series names from the header row. Its
/// size is given in pixels rather than as a cell span, so the anchor is converted at Excel's
/// default 64 × 20 px cell size.</para>
/// </summary>
public class OfficeImoReportBenchmarks
{
    private const int DefaultColumnWidthPx = 64;
    private const int DefaultRowHeightPx = 20;

    [GlobalSetup]
    public void Setup() => StockData.EnsureLoaded();

    [Benchmark]
    public void CreateStockReport()
    {
        using var ms = new MemoryStream();
        Build(ms);
    }

    [GlobalCleanup]
    public void SaveArtifact() => WriteArtifact();

    /// <summary>Writes the artifact and reports whether it landed (false = target file locked).</summary>
    internal static bool WriteArtifact() => ReportOutput.Save("officeimo", Build);

    private static void Build(Stream output)
    {
        using var doc = ExcelDocument.Create();
        var ws = doc.AddSheet(ReportLayout.SheetName);

        WriteData(ws);
        AddConditionalFormatting(ws);
        AddChart(ws);

        doc.Save(output);
    }

    private static void WriteData(ExcelSheet ws)
    {
        ws.Cell(ReportLayout.HeaderRow, ReportLayout.WeekNoCol).SetValue("Week");
        ws.Cell(ReportLayout.HeaderRow, ReportLayout.WeekEndingCol).SetValue(ReportLayout.WeekEndingHeader);
        for (var s = 0; s < StockData.SymbolCount; s++)
            ws.Cell(ReportLayout.HeaderRow, ReportLayout.FirstPriceCol + s).SetValue(StockData.Symbols[s]);

        for (var col = ReportLayout.WeekNoCol; col <= ReportLayout.LastPriceCol; col++)
            ws.Cell(ReportLayout.HeaderRow, col).SetBold();

        for (var w = 0; w < StockData.WeekCount; w++)
        {
            var row = ReportLayout.FirstDataRow + w;
            ws.Cell(row, ReportLayout.WeekNoCol).SetValue(w + 1);

            ws.Cell(row, ReportLayout.WeekEndingCol).SetValue(StockData.WeekEndings[w]);
            ws.Cell(row, ReportLayout.WeekEndingCol).SetNumberFormat(ReportLayout.DateFormat);

            var prices = StockData.Prices[w];
            for (var s = 0; s < prices.Length; s++)
            {
                var col = ReportLayout.FirstPriceCol + s;
                ws.Cell(row, col).SetValue(prices[s]);
                ws.Cell(row, col).SetNumberFormat(ReportLayout.PriceFormat);
            }
        }

        // Auto-fit the week-ending column. This measures the rendered text with a font engine,
        // so it is the one part of the scenario whose cost is text shaping rather than XML.
        ws.AutoFitColumn(ReportLayout.WeekEndingCol);
    }

    private static void AddConditionalFormatting(ExcelSheet ws)
    {
        ws.AddConditionalFormattingRule(new ExcelConditionalFormattingInfo
        {
            Range = ReportLayout.CfRangeA1,
            Type = "expression",
            Formulas = [ReportLayout.UpFormula],
            DifferentialFillColorArgb = ReportLayout.Hex(ReportLayout.UpFill),
            DifferentialFontColorArgb = ReportLayout.Hex(ReportLayout.UpFont),
        });

        ws.AddConditionalFormattingRule(new ExcelConditionalFormattingInfo
        {
            Range = ReportLayout.CfRangeA1,
            Type = "expression",
            Formulas = [ReportLayout.DownFormula],
            DifferentialFillColorArgb = ReportLayout.Hex(ReportLayout.DownFill),
            DifferentialFontColorArgb = ReportLayout.Hex(ReportLayout.DownFont),
        });
    }

    private static void AddChart(ExcelSheet ws)
    {
        // Categories in the week-ending column, then one series per price column to its right.
        var data = new ExcelChartDataRange(
            ReportLayout.SheetName,
            startRow: ReportLayout.HeaderRow,
            startColumn: ReportLayout.WeekEndingCol,
            categoryCount: StockData.WeekCount,
            seriesCount: StockData.SymbolCount,
            hasHeaderRow: true);

        var chart = ws.AddChart(OfficeChartType.Line, data, new ExcelChartOptions
        {
            Row = ReportLayout.ChartFirstRow,
            Column = ReportLayout.ChartFirstCol,
            Width = OfficeLength.Pixels(ReportLayout.ChartColSpan * DefaultColumnWidthPx),
            Height = OfficeLength.Pixels(ReportLayout.ChartRowSpan * DefaultRowHeightPx),
            Title = ReportLayout.ChartTitle,
        });

        chart.SetLegend(OfficeChartLegendPosition.Right);
    }
}
