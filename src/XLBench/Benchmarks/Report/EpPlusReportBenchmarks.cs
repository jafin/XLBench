using System.Drawing;
using BenchmarkDotNet.Attributes;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.Report;

/// <summary>
/// EPPlus stock report: data + conditional formatting + line chart — full scenario support.
///
/// EPPlus has the most direct API of the six for this scenario: typed expression rules with a
/// differential-format style object, and a first-class chart model where a series binds to
/// worksheet ranges rather than to A1 strings.
/// </summary>
public class EpPlusReportBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        EpPlusLicense.Ensure();
        StockData.EnsureLoaded();
    }

    [Benchmark]
    public void CreateStockReport()
    {
        using var ms = new MemoryStream();
        Build(ms);
    }

    [GlobalCleanup]
    public void SaveArtifact() => WriteArtifact();

    /// <summary>Writes the artifact and reports whether it landed (false = target file locked).</summary>
    internal static bool WriteArtifact()
    {
        // Can run without GlobalSetup — when invoked via `dotnet run -- report`, or when
        // BenchmarkDotNet filters the benchmark out but still runs cleanup.
        EpPlusLicense.Ensure();
        return ReportOutput.Save("epplus", Build);
    }

    private static void Build(Stream output)
    {
        using var pkg = new ExcelPackage();
        var ws = pkg.Workbook.Worksheets.Add(ReportLayout.SheetName);

        WriteData(ws);
        AddConditionalFormatting(ws);
        AddChart(ws);

        pkg.SaveAs(output);
    }

    private static void WriteData(ExcelWorksheet ws)
    {
        ws.Cells[ReportLayout.HeaderRow, ReportLayout.WeekNoCol].Value = "Week";
        ws.Cells[ReportLayout.HeaderRow, ReportLayout.WeekEndingCol].Value = "Week Ending";
        for (var s = 0; s < StockData.SymbolCount; s++)
            ws.Cells[ReportLayout.HeaderRow, ReportLayout.FirstPriceCol + s].Value = StockData.Symbols[s];

        ws.Cells[ReportLayout.HeaderRow, 1, ReportLayout.HeaderRow, ReportLayout.LastPriceCol]
            .Style.Font.Bold = true;

        for (var w = 0; w < StockData.WeekCount; w++)
        {
            var row = ReportLayout.FirstDataRow + w;
            ws.Cells[row, ReportLayout.WeekNoCol].Value = w + 1;
            ws.Cells[row, ReportLayout.WeekEndingCol].Value = StockData.WeekEndings[w];

            var prices = StockData.Prices[w];
            for (var s = 0; s < prices.Length; s++)
                ws.Cells[row, ReportLayout.FirstPriceCol + s].Value = prices[s];
        }

        // Column-level number formats — cheaper and more idiomatic than per-cell in EPPlus.
        ws.Cells[ReportLayout.FirstDataRow, ReportLayout.WeekEndingCol, ReportLayout.LastDataRow, ReportLayout.WeekEndingCol]
            .Style.Numberformat.Format = ReportLayout.DateFormat;
        ws.Cells[ReportLayout.FirstDataRow, ReportLayout.FirstPriceCol, ReportLayout.LastDataRow, ReportLayout.LastPriceCol]
            .Style.Numberformat.Format = ReportLayout.PriceFormat;
    }

    private static void AddConditionalFormatting(ExcelWorksheet ws)
    {
        var address = new ExcelAddress(
            ReportLayout.CfFirstRow, ReportLayout.FirstPriceCol,
            ReportLayout.LastDataRow, ReportLayout.LastPriceCol);

        var up = ws.ConditionalFormatting.AddExpression(address);
        up.Formula = ReportLayout.UpFormula;
        up.Style.Fill.PatternType = ExcelFillStyle.Solid;
        up.Style.Fill.BackgroundColor.Color = Rgb(ReportLayout.UpFill);
        up.Style.Font.Color.Color = Rgb(ReportLayout.UpFont);

        var down = ws.ConditionalFormatting.AddExpression(address);
        down.Formula = ReportLayout.DownFormula;
        down.Style.Fill.PatternType = ExcelFillStyle.Solid;
        down.Style.Fill.BackgroundColor.Color = Rgb(ReportLayout.DownFill);
        down.Style.Font.Color.Color = Rgb(ReportLayout.DownFont);
    }

    private static void AddChart(ExcelWorksheet ws)
    {
        var chart = ws.Drawings.AddLineChart("WeeklyPrices", eLineChartType.Line);
        chart.Title.Text = ReportLayout.ChartTitle;
        chart.SetPosition(ReportLayout.ChartFirstRow - 1, 0, ReportLayout.ChartFirstCol - 1, 0);
        chart.SetSize(900, 500);

        var categories = ws.Cells[
            ReportLayout.FirstDataRow, ReportLayout.WeekEndingCol,
            ReportLayout.LastDataRow, ReportLayout.WeekEndingCol];

        for (var s = 0; s < StockData.SymbolCount; s++)
        {
            var col = ReportLayout.FirstPriceCol + s;
            var values = ws.Cells[ReportLayout.FirstDataRow, col, ReportLayout.LastDataRow, col];
            var serie = chart.Series.Add(values, categories);
            serie.Header = StockData.Symbols[s];
        }
    }

    private static Color Rgb((byte R, byte G, byte B) c) => Color.FromArgb(c.R, c.G, c.B);
}
