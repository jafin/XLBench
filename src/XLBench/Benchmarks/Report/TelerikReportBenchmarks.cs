using BenchmarkDotNet.Attributes;
using Telerik.Documents.Common.Model;
using Telerik.Documents.Media;
using Telerik.Documents.Model.Drawing.Charts;
using Telerik.Documents.Spreadsheet.FormatProviders.OpenXml.Xlsx;
using Telerik.Documents.Spreadsheet.Model;
using Telerik.Documents.Spreadsheet.Model.Charts;
using Telerik.Documents.Spreadsheet.Model.ConditionalFormattings;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.Report;

/// <summary>
/// RadSpreadProcessing stock report: data + conditional formatting + line chart — full scenario
/// support, and the chart is the least code of any library here.
///
/// <para><b>Chart.</b> <c>ChartCollection.Add</c> takes an anchor cell, one data range covering
/// the headers and the values, and a chart type; the series and their names are inferred from the
/// range. There is nothing to bind per symbol, so where the other libraries loop over twenty
/// series references this is a single call. The chart is sized in device-independent pixels
/// rather than by a second cell anchor, so the span the other benchmarks express as 14 columns ×
/// 30 rows is converted here from Excel's default column width and row height.</para>
///
/// <para><b>Number formats are applied per range, not per cell</b> — the same shape as the EPPlus
/// and IronXL benchmarks. Telerik stores cell properties sparsely over ranges, so writing the
/// price format 5,200 times would measure the property system rather than the scenario.</para>
///
/// <para><b>Auto-fit</b> goes through <c>Telerik.Documents.Core.TextMeasurer</c>, which ships in
/// the box — so, like ClosedXML, EPPlus and XLibur, this measures real text shaping.</para>
/// </summary>
public class TelerikReportBenchmarks
{
    /// <summary>Excel's default column width and row height in DIPs, for sizing the chart.</summary>
    private const double DefaultColumnWidthPx = 64d;
    private const double DefaultRowHeightPx = 20d;

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
    internal static bool WriteArtifact() => ReportOutput.Save("telerik", Build);

    private static void Build(Stream output)
    {
        using var workbook = new Workbook().WithoutHistory();
        var worksheet = workbook.Worksheets.Add();
        worksheet.Name = ReportLayout.SheetName;

        WriteData(worksheet);
        AddConditionalFormatting(worksheet);
        AddChart(worksheet);

        new XlsxFormatProvider().Export(workbook, output, null);
    }

    private static void WriteData(Worksheet ws)
    {
        var headerRow = ReportLayout.HeaderRow - 1;
        var weekNoCol = ReportLayout.WeekNoCol - 1;
        var weekEndingCol = ReportLayout.WeekEndingCol - 1;
        var firstPriceCol = ReportLayout.FirstPriceCol - 1;

        ws.Cells[headerRow, weekNoCol].SetValue("Week");
        ws.Cells[headerRow, weekEndingCol].SetValue(ReportLayout.WeekEndingHeader);
        for (var s = 0; s < StockData.SymbolCount; s++)
            ws.Cells[headerRow, firstPriceCol + s].SetValue(StockData.Symbols[s]);

        ws.Cells[headerRow, weekNoCol, headerRow, ReportLayout.LastPriceCol - 1].SetIsBold(true);

        for (var w = 0; w < StockData.WeekCount; w++)
        {
            var row = ReportLayout.FirstDataRow - 1 + w;
            ws.Cells[row, weekNoCol].SetValue((double)(w + 1));
            ws.Cells[row, weekEndingCol].SetValue(StockData.WeekEndings[w]);

            var prices = StockData.Prices[w];
            for (var s = 0; s < prices.Length; s++)
                ws.Cells[row, firstPriceCol + s].SetValue(prices[s]);
        }

        ws.Cells[ReportLayout.FirstDataRow - 1, weekEndingCol, ReportLayout.LastDataRow - 1, weekEndingCol]
            .SetFormat(new CellValueFormat(ReportLayout.DateFormat));
        ws.Cells[ReportLayout.FirstDataRow - 1, firstPriceCol, ReportLayout.LastDataRow - 1, ReportLayout.LastPriceCol - 1]
            .SetFormat(new CellValueFormat(ReportLayout.PriceFormat));

        // Auto-fit the week-ending column. This measures the rendered text with a font engine,
        // so it is the one part of the scenario whose cost is text shaping rather than XML.
        ws.Columns[weekEndingCol].AutoFitWidth();
    }

    private static void AddConditionalFormatting(Worksheet ws)
    {
        var block = new CellRange(
            ReportLayout.CfFirstRow - 1, ReportLayout.FirstPriceCol - 1,
            ReportLayout.LastDataRow - 1, ReportLayout.LastPriceCol - 1);
        var selection = ws.Cells[block];

        selection.AddConditionalFormatting(new ConditionalFormatting(
            new FormulaRule($"={ReportLayout.UpFormula}", Formatting(ReportLayout.UpFill, ReportLayout.UpFont))));
        selection.AddConditionalFormatting(new ConditionalFormatting(
            new FormulaRule($"={ReportLayout.DownFormula}", Formatting(ReportLayout.DownFill, ReportLayout.DownFont))));
    }

    private static DifferentialFormatting Formatting((byte R, byte G, byte B) fill, (byte R, byte G, byte B) font) =>
        new()
        {
            Fill = new PatternFill(PatternType.Solid, Color(fill), Colors.Transparent),
            ForeColor = new ThemableColor(Color(font)),
        };

    private static void AddChart(Worksheet ws)
    {
        // One range over the price block, headers included: each column becomes a series named
        // from its header. The week-ending column is deliberately left out of it — Telerik reads
        // an extra leading column as a twenty-first series rather than as the category axis, so
        // the dates are bound afterwards as each series' Categories instead.
        var prices = new CellRange(
            ReportLayout.HeaderRow - 1, ReportLayout.FirstPriceCol - 1,
            ReportLayout.LastDataRow - 1, ReportLayout.LastPriceCol - 1);
        var anchor = new CellIndex(ReportLayout.ChartFirstRow - 1, ReportLayout.ChartFirstCol - 1);

        var shape = ws.Charts.Add(anchor, prices, ChartType.Line);
        shape.Chart.Title = new TextTitle(ReportLayout.ChartTitle);
        shape.Chart.Legend = new Legend { Position = LegendPosition.Right };

        var categories = new WorkbookFormulaChartData(ws, new CellRange(
            ReportLayout.FirstDataRow - 1, ReportLayout.WeekEndingCol - 1,
            ReportLayout.LastDataRow - 1, ReportLayout.WeekEndingCol - 1));
        foreach (var group in shape.Chart.SeriesGroups)
        foreach (var series in group.Series.OfType<LineSeries>())
            series.Categories = categories;

        shape.SetWidth(respectLockAspectRatio: false, ReportLayout.ChartColSpan * DefaultColumnWidthPx, adjustCellIndex: false);
        shape.SetHeight(respectLockAspectRatio: false, ReportLayout.ChartRowSpan * DefaultRowHeightPx, adjustCellIndex: false);
    }

    private static Color Color((byte R, byte G, byte B) c) =>
        Telerik.Documents.Media.Color.FromRgb(c.R, c.G, c.B);
}
