using BenchmarkDotNet.Attributes;
using IronXL;
using IronXL.Drawing.Charts;
using IronXL.Formatting;
using IronXL.Formatting.Enums;
using IronXL.Styles;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.Report;

/// <summary>
/// IronXL stock report: data + conditional formatting + auto-fit + line chart.
///
/// IronXL wraps NPOI internally (its own XML docs reference <c>NPOI.SS.Formula.Formula</c>),
/// so the feature set closely tracks NPOI's while the surface API is higher level: rules are
/// created from the sheet's <c>ConditionalFormatting</c> object and charts take A1 range
/// strings rather than a CT model.
/// </summary>
public class IronXlReportBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        IronXlLicense.Ensure();
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
        IronXlLicense.Ensure();
        return ReportOutput.Save("ironxl", Build);
    }

    private static void Build(Stream output)
    {
        var wb = WorkBook.Create(ExcelFileFormat.XLSX);
        var ws = wb.CreateWorkSheet(ReportLayout.SheetName);

        WriteData(ws);
        AddConditionalFormatting(ws);
        AddChart(ws);

        // IronXL has no SaveAs(Stream): ToStream() builds its own MemoryStream and hands it
        // back, so the full workbook is buffered in memory whatever the real destination is.
        using var ms = wb.ToStream();
        ms.CopyTo(output);
    }

    private static void WriteData(WorkSheet ws)
    {
        // SetCellValue is 0-based; the shared layout constants are 1-based.
        ws.SetCellValue(ReportLayout.HeaderRow - 1, ReportLayout.WeekNoCol - 1, "Week");
        ws.SetCellValue(ReportLayout.HeaderRow - 1, ReportLayout.WeekEndingCol - 1, ReportLayout.WeekEndingHeader);
        for (var s = 0; s < StockData.SymbolCount; s++)
            ws.SetCellValue(ReportLayout.HeaderRow - 1, ReportLayout.FirstPriceCol - 1 + s, StockData.Symbols[s]);

        ws[HeaderRangeA1].Style.Font.Bold = true;

        for (var w = 0; w < StockData.WeekCount; w++)
        {
            var row = ReportLayout.FirstDataRow - 1 + w;
            ws.SetCellValue(row, ReportLayout.WeekNoCol - 1, w + 1);
            ws.SetCellValue(row, ReportLayout.WeekEndingCol - 1, StockData.WeekEndings[w]);

            var prices = StockData.Prices[w];
            for (var s = 0; s < prices.Length; s++)
                ws.SetCellValue(row, ReportLayout.FirstPriceCol - 1 + s, prices[s]);
        }

        // Range-level number formats, matching the EPPlus/XLibur shape of the scenario.
        ws[DateRangeA1].FormatString = ReportLayout.DateFormat;
        ws[PriceRangeA1].FormatString = ReportLayout.PriceFormat;

        // Auto-fit the week-ending column. This measures the rendered text with a font engine,
        // so it is the one part of the scenario whose cost is text shaping rather than XML.
        ws.AutoSizeColumn(ReportLayout.WeekEndingCol - 1);
    }

    private static void AddConditionalFormatting(WorkSheet ws)
    {
        AddRule(ws, ReportLayout.UpFormula, ReportLayout.UpFill, ReportLayout.UpFont);
        AddRule(ws, ReportLayout.DownFormula, ReportLayout.DownFill, ReportLayout.DownFont);
    }

    private static void AddRule(WorkSheet ws, string formula,
        (byte R, byte G, byte B) fill, (byte R, byte G, byte B) font)
    {
        var cf = ws.ConditionalFormatting;
        var rule = cf.CreateConditionalFormattingRule(formula);

        // The fill is asked for the same way the other libraries are asked, and IronXL drops it
        // on the floor — see the note on Rgb/Argb below. Left in place deliberately: the
        // scenario is "ask each library for a green/red conditional fill", and what IronXL does
        // with that request is the result.
        rule.PatternFormatting.FillPattern = FillPattern.SolidForeground;
        rule.PatternFormatting.BackgroundColor = Rgb(fill);
        rule.FontFormatting.FontColor = Argb(font);

        cf.AddConditionalFormatting(ReportLayout.CfRangeA1, rule);
    }

    private static void AddChart(WorkSheet ws)
    {
        var chart = ws.CreateChart(
            ChartType.Line,
            ReportLayout.ChartFirstRow - 1,
            ReportLayout.ChartFirstCol - 1,
            ReportLayout.ChartFirstRow - 1 + ReportLayout.ChartRowSpan,
            ReportLayout.ChartFirstCol - 1 + ReportLayout.ChartColSpan);

        var categories = LocalRef(ReportLayout.WeekEndingCol);
        for (var s = 0; s < StockData.SymbolCount; s++)
        {
            var series = chart.AddSeries(categories, LocalRef(ReportLayout.FirstPriceCol + s));
            series.Title = StockData.Symbols[s];
        }

        chart.SetTitle(ReportLayout.ChartTitle);
        chart.SetLegendPosition(LegendPosition.Right);

        // Nothing is written to the chart part until Plot() commits the accumulated series.
        chart.Plot();
    }

    /// <summary>Unqualified A1 range for a whole data column, e.g. <c>C2:C261</c>.</summary>
    private static string LocalRef(int column)
    {
        var col = ReportLayout.ColumnLetter(column);
        return $"{col}{ReportLayout.FirstDataRow}:{col}{ReportLayout.LastDataRow}";
    }

    private static string HeaderRangeA1 =>
        $"A{ReportLayout.HeaderRow}:{ReportLayout.ColumnLetter(ReportLayout.LastPriceCol)}{ReportLayout.HeaderRow}";

    private static string DateRangeA1 =>
        $"{ReportLayout.ColumnLetter(ReportLayout.WeekEndingCol)}{ReportLayout.FirstDataRow}:" +
        $"{ReportLayout.ColumnLetter(ReportLayout.WeekEndingCol)}{ReportLayout.LastDataRow}";

    private static string PriceRangeA1 =>
        $"{ReportLayout.ColumnLetter(ReportLayout.FirstPriceCol)}{ReportLayout.FirstDataRow}:" +
        $"{ReportLayout.ColumnLetter(ReportLayout.LastPriceCol)}{ReportLayout.LastDataRow}";

    /// <summary>
    /// <c>#RRGGBB</c>, for <see cref="IPatternFormatting.BackgroundColor"/>.
    /// </summary>
    /// <remarks>
    /// The value is accepted and discarded. IronXL converts the string to a <c>short</c> legacy
    /// palette index before handing it to the NPOI pattern-formatting model it wraps, and every
    /// colour tried — pastel or primary, 6-digit or 8-digit — comes out as
    /// <c>&lt;bgColor indexed="0"/&gt;</c> (black). The property getter reads back the string
    /// from IronXL's own cache rather than from the model, so it round-trips convincingly while
    /// the workbook gets none of it. There is no other public API for a conditional-format fill.
    /// </remarks>
    private static string Rgb((byte R, byte G, byte B) c) => $"#{ReportLayout.Hex(c)}";

    /// <summary>
    /// <c>#AARRGGBB</c>, for <see cref="IFontFormatting.FontColor"/> — which, unlike the pattern
    /// fill, does reach the file.
    /// </summary>
    /// <remarks>
    /// The two properties disagree on format. FontColor writes the digits through verbatim, so a
    /// 6-digit value lands as <c>rgb="006100"</c> and fails schema validation (OOXML requires
    /// 4-byte ARGB); the alpha byte is required here. BackgroundColor does the opposite and
    /// truncates to 6 digits, reading <c>#FFFF0000</c> (opaque red) back as <c>#ffff00</c>.
    /// </remarks>
    private static string Argb((byte R, byte G, byte B) c) => $"#FF{ReportLayout.Hex(c)}";
}
