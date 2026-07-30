using System.Globalization;
using BenchmarkDotNet.Attributes;
using NPOI.OpenXmlFormats.Dml.Chart;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XDDF.UserModel.Chart;
using NPOI.XSSF.UserModel;
using XLBench.Data;

namespace XLBench.Benchmarks.Report;

/// <summary>
/// NPOI stock report: data + conditional formatting + line chart — full scenario support.
///
/// Charts come from the XDDF model (NPOI's port of Apache POI's DrawingML layer), which is
/// noticeably more ceremonious than the other libraries: axes are created explicitly, series
/// are attached to a chart-data object, and the whole thing is only committed on
/// <c>chart.Plot(data)</c>.
///
/// Category labels are supplied via <c>FromArray</c> rather than <c>FromStringCellRange</c>
/// because the week-ending column holds real date cells, which the string-range reader cannot
/// project. The A1 range is still passed alongside, so the chart stays bound to the sheet.
/// </summary>
public class NpoiReportBenchmarks
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
    internal static bool WriteArtifact() => ReportOutput.Save("npoi", Build);

    private static void Build(Stream output)
    {
        using var wb = new XSSFWorkbook();
        var sheet = (XSSFSheet)wb.CreateSheet(ReportLayout.SheetName);

        WriteData(wb, sheet);
        AddConditionalFormatting(sheet);
        AddChart(sheet);

        wb.Write(output, leaveOpen: true);
    }

    private static void WriteData(XSSFWorkbook wb, ISheet sheet)
    {
        var format = wb.CreateDataFormat();

        var headerStyle = wb.CreateCellStyle();
        var headerFont = wb.CreateFont();
        headerFont.IsBold = true;
        headerStyle.SetFont(headerFont);

        var dateStyle = wb.CreateCellStyle();
        dateStyle.DataFormat = format.GetFormat(ReportLayout.DateFormat);

        var priceStyle = wb.CreateCellStyle();
        priceStyle.DataFormat = format.GetFormat(ReportLayout.PriceFormat);

        // NPOI rows/cols are 0-based; the layout constants are 1-based A1 positions.
        var header = sheet.CreateRow(ReportLayout.HeaderRow - 1);
        SetString(header, ReportLayout.WeekNoCol - 1, "Week", headerStyle);
        SetString(header, ReportLayout.WeekEndingCol - 1, ReportLayout.WeekEndingHeader, headerStyle);
        for (var s = 0; s < StockData.SymbolCount; s++)
            SetString(header, ReportLayout.FirstPriceCol - 1 + s, StockData.Symbols[s], headerStyle);

        for (var w = 0; w < StockData.WeekCount; w++)
        {
            var row = sheet.CreateRow(ReportLayout.FirstDataRow - 1 + w);
            row.CreateCell(ReportLayout.WeekNoCol - 1).SetCellValue(w + 1);

            var dateCell = row.CreateCell(ReportLayout.WeekEndingCol - 1);
            dateCell.SetCellValue(StockData.WeekEndings[w]);
            dateCell.CellStyle = dateStyle;

            var prices = StockData.Prices[w];
            for (var s = 0; s < prices.Length; s++)
            {
                var cell = row.CreateCell(ReportLayout.FirstPriceCol - 1 + s);
                cell.SetCellValue(prices[s]);
                cell.CellStyle = priceStyle;
            }
        }

        // Auto-fit the week-ending column. This measures the rendered text with a font engine,
        // so it is the one part of the scenario whose cost is text shaping rather than XML.
        sheet.AutoSizeColumn(ReportLayout.WeekEndingCol - 1);
    }

    private static void SetString(IRow row, int col, string value, ICellStyle style)
    {
        var cell = row.CreateCell(col);
        cell.SetCellValue(value);
        cell.CellStyle = style;
    }

    private static void AddConditionalFormatting(ISheet sheet)
    {
        var scf = sheet.SheetConditionalFormatting;

        var up = scf.CreateConditionalFormattingRule(ReportLayout.UpFormula);
        StylePattern(up, ReportLayout.UpFill, ReportLayout.UpFont);

        var down = scf.CreateConditionalFormattingRule(ReportLayout.DownFormula);
        StylePattern(down, ReportLayout.DownFill, ReportLayout.DownFont);

        var region = new CellRangeAddress(
            ReportLayout.CfFirstRow - 1, ReportLayout.LastDataRow - 1,
            ReportLayout.FirstPriceCol - 1, ReportLayout.LastPriceCol - 1);

        scf.AddConditionalFormatting([region], [up, down]);
    }

    private static void StylePattern(
        IConditionalFormattingRule rule,
        (byte R, byte G, byte B) fill,
        (byte R, byte G, byte B) font)
    {
        var pattern = rule.CreatePatternFormatting();
        // POI convention: a solid CF fill is expressed as SolidForeground + a background colour.
        pattern.FillBackgroundColorColor = Color(fill);
        pattern.FillPattern = FillPattern.SolidForeground;

        rule.CreateFontFormatting().FontColor = Color(font);
    }

    private static void AddChart(XSSFSheet sheet)
    {
        var drawing = (XSSFDrawing)sheet.CreateDrawingPatriarch();
        var anchor = drawing.CreateAnchor(
            0, 0, 0, 0,
            ReportLayout.ChartFirstCol - 1, ReportLayout.ChartFirstRow - 1,
            ReportLayout.ChartFirstCol - 1 + ReportLayout.ChartColSpan,
            ReportLayout.ChartFirstRow - 1 + ReportLayout.ChartRowSpan);

        var chart = drawing.CreateChart(anchor);

        // No chart title. NPOI 2.8.0's SetTitleText serializes the rich-text body as <a:rich>
        // (DrawingML main namespace) where the chart schema requires <c:rich>; CT_Tx.Write hands
        // the CT_TextBody its own "a" prefix unconditionally, so no public API avoids it. The
        // resulting file makes Excel prompt to repair, which would defeat keeping these artifacts
        // around for review — so the title is left off and the gap is recorded in the README's
        // capability matrix instead.
        chart.SetAutoTitleDeleted(true);
        chart.GetOrAddLegend().Position = LegendPosition.Right;

        var categoryAxis = chart.CreateCategoryAxis(AxisPosition.Bottom);
        var valueAxis = chart.CreateValueAxis(AxisPosition.Left);
        valueAxis.Crosses = AxisCrosses.AutoZero;

        var labels = StockData.WeekEndings
            .Select(d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            .ToArray();
        var categories = XDDFDataSourcesFactory.FromArray(labels, ReportLayout.CategoryRef());

        var data = chart.CreateData<string, double>(ChartTypes.LINE, categoryAxis, valueAxis);
        SetLineGrouping(chart);

        for (var s = 0; s < StockData.SymbolCount; s++)
        {
            var col = ReportLayout.FirstPriceCol - 1 + s;
            var values = XDDFDataSourcesFactory.FromNumericCellRange(sheet, new CellRangeAddress(
                ReportLayout.FirstDataRow - 1, ReportLayout.LastDataRow - 1, col, col));

            var series = data.AddSeries(categories, values);
            series.SetTitle(StockData.Symbols[s], null);
        }

        chart.Plot(data);
    }

    /// <summary>
    /// Sets <c>&lt;c:grouping&gt;</c> on the generated line chart, which the schema requires (and
    /// Excel expects) as the first child of <c>&lt;c:lineChart&gt;</c>.
    ///
    /// NPOI 2.8.0 never creates the element: <c>XDDFLineChartData.SetGrouping</c> assigns
    /// straight through a null <c>grouping</c> field and throws a <see cref="NullReferenceException"/>,
    /// so the only way to get a valid chart is to reach past XDDF to the underlying CT model.
    /// Insertion order does not matter — <c>CT_LineChart.Write</c> emits in schema order.
    /// </summary>
    private static void SetLineGrouping(XDDFChart chart)
    {
        foreach (var lineChart in chart.GetCTChart().plotArea.lineChart)
            lineChart.grouping = new CT_Grouping { val = ST_Grouping.standard };
    }

    // Four bytes, not three: XSSFColor stores the array verbatim, and a 3-byte RGB serializes to
    // a 6-character rgb attribute, which is invalid against the schema (hexBinary wants ARGB).
    private static XSSFColor Color((byte R, byte G, byte B) c) =>
        new([0xFF, c.R, c.G, c.B], null);
}
