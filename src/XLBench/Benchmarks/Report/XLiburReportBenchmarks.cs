using BenchmarkDotNet.Attributes;
using XLBench.Data;
using XLibur.Excel;

namespace XLBench.Benchmarks.Report;

/// <summary>
/// XLibur stock report: data + conditional formatting + line chart — full scenario support.
///
/// The cell/conditional-format API is ClosedXML-shaped, so that half mirrors
/// <see cref="ClosedXmlReportBenchmarks"/> exactly. Unlike ClosedXML, XLibur exposes charts
/// publicly via <c>IXLWorksheet.Charts</c>, with series bound to sheet-qualified A1 references.
///
/// <para>Legends are opt-in here: a chart XLibur creates has none until <c>Legend.Visible</c> is
/// set, where EPPlus adds one by default and NPOI and the OpenXML SDK are asked explicitly. All
/// four are configured to show one on the right so the artifacts are comparable.</para>
/// </summary>
public class XLiburReportBenchmarks
{
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
    internal static bool WriteArtifact() => ReportOutput.Save("xlibur", Build);

    private static void Build(Stream output)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet(ReportLayout.SheetName);

        WriteData(ws);
        AddConditionalFormatting(ws);
        AddChart(ws);

        wb.SaveAs(output);
    }

    private static void WriteData(IXLWorksheet ws)
    {
        ws.Cell(ReportLayout.HeaderRow, ReportLayout.WeekNoCol).Value = "Week";
        ws.Cell(ReportLayout.HeaderRow, ReportLayout.WeekEndingCol).Value = ReportLayout.WeekEndingHeader;
        for (var s = 0; s < StockData.SymbolCount; s++)
            ws.Cell(ReportLayout.HeaderRow, ReportLayout.FirstPriceCol + s).Value = StockData.Symbols[s];

        ws.Row(ReportLayout.HeaderRow).Style.Font.Bold = true;

        for (var w = 0; w < StockData.WeekCount; w++)
        {
            var row = ReportLayout.FirstDataRow + w;
            ws.Cell(row, ReportLayout.WeekNoCol).Value = w + 1;

            var dateCell = ws.Cell(row, ReportLayout.WeekEndingCol);
            dateCell.Value = StockData.WeekEndings[w];
            dateCell.Style.DateFormat.Format = ReportLayout.DateFormat;

            var prices = StockData.Prices[w];
            for (var s = 0; s < prices.Length; s++)
            {
                var cell = ws.Cell(row, ReportLayout.FirstPriceCol + s);
                cell.Value = prices[s];
                cell.Style.NumberFormat.Format = ReportLayout.PriceFormat;
            }
        }

        // Auto-fit the week-ending column. This measures the rendered text with a font engine,
        // so it is the one part of the scenario whose cost is text shaping rather than XML.
        ws.Column(ReportLayout.WeekEndingCol).AdjustToContents();
    }

    private static void AddConditionalFormatting(IXLWorksheet ws)
    {
        var range = ws.Range(
            ReportLayout.CfFirstRow, ReportLayout.FirstPriceCol,
            ReportLayout.LastDataRow, ReportLayout.LastPriceCol);

        var up = range.AddConditionalFormat().WhenIsTrue($"={ReportLayout.UpFormula}");
        up.Fill.SetBackgroundColor(Color(ReportLayout.UpFill));
        up.Font.SetFontColor(Color(ReportLayout.UpFont));

        var down = range.AddConditionalFormat().WhenIsTrue($"={ReportLayout.DownFormula}");
        down.Fill.SetBackgroundColor(Color(ReportLayout.DownFill));
        down.Font.SetFontColor(Color(ReportLayout.DownFont));
    }

    private static void AddChart(IXLWorksheet ws)
    {
        var chart = ws.Charts.Add(XLChartType.Line);
        chart.SetTitle(ReportLayout.ChartTitle);
        chart.Legend.Visible = true;
        chart.Legend.Position = XLLegendPosition.Right;

        chart.Position.Column = ReportLayout.ChartFirstCol;
        chart.Position.Row = ReportLayout.ChartFirstRow;
        chart.SecondPosition.Column = ReportLayout.ChartFirstCol + ReportLayout.ChartColSpan;
        chart.SecondPosition.Row = ReportLayout.ChartFirstRow + ReportLayout.ChartRowSpan;

        var categories = ReportLayout.CategoryRef();
        for (var s = 0; s < StockData.SymbolCount; s++)
            chart.Series.Add(StockData.Symbols[s], ReportLayout.PriceColumnRef(s), categories);
    }

    private static XLColor Color((byte R, byte G, byte B) c) => XLColor.FromArgb(c.R, c.G, c.B);
}
