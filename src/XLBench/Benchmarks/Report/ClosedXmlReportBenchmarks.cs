using BenchmarkDotNet.Attributes;
using ClosedXML.Excel;
using XLBench.Data;

namespace XLBench.Benchmarks.Report;

/// <summary>
/// ClosedXML stock report: data + conditional formatting only.
///
/// **Chart: not supported.** ClosedXML 0.105.0 carries internal <c>XLChart</c>/<c>XLCharts</c>
/// types, but nothing exposes them — <c>IXLWorksheet</c> has no <c>Charts</c> member, so there
/// is no public API to add one. This benchmark therefore does strictly less work than the
/// libraries that also emit a chart; see the capability matrix in the README before comparing
/// its number against theirs.
/// </summary>
public class ClosedXmlReportBenchmarks
{
    [Benchmark]
    public void CreateStockReport()
    {
        using var ms = new MemoryStream();
        Build(ms);
    }

    [GlobalCleanup]
    public void SaveArtifact() => WriteArtifact();

    /// <summary>Writes the artifact and reports whether it landed (false = target file locked).</summary>
    internal static bool WriteArtifact() => ReportOutput.Save("closedxml", Build);

    private static void Build(Stream output)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet(ReportLayout.SheetName);

        WriteData(ws);
        AddConditionalFormatting(ws);

        wb.SaveAs(output);
    }

    private static void WriteData(IXLWorksheet ws)
    {
        ws.Cell(ReportLayout.HeaderRow, ReportLayout.WeekNoCol).Value = "Week";
        ws.Cell(ReportLayout.HeaderRow, ReportLayout.WeekEndingCol).Value = "Week Ending";
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

    private static XLColor Color((byte R, byte G, byte B) c) => XLColor.FromArgb(c.R, c.G, c.B);
}
