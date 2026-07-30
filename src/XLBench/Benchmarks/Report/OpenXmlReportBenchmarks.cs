using System.Globalization;
using BenchmarkDotNet.Attributes;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using XLBench.Data;
using A = DocumentFormat.OpenXml.Drawing;
using C = DocumentFormat.OpenXml.Drawing.Charts;
using Xdr = DocumentFormat.OpenXml.Drawing.Spreadsheet;

namespace XLBench.Benchmarks.Report;

/// <summary>
/// OpenXML SDK stock report: data + conditional formatting + line chart — full scenario support,
/// at the cost of hand-authoring every part of the package.
///
/// Nothing here is convenience API: the stylesheet (including the two differential formats the
/// conditional-format rules point at), the <c>conditionalFormatting</c> element, the drawing
/// part's two-cell anchor, and the entire chart part's DrawingML are all built element by
/// element. That is the honest shape of this scenario on the raw SDK — it can do everything the
/// higher-level libraries do, but the authoring cost is an order of magnitude higher, which is
/// exactly what this benchmark is meant to surface alongside the timings.
///
/// The DOM API is used rather than the SAX <c>OpenXmlWriter</c> used by the large write
/// benchmark: this sheet is tiny (52 rows), and charts and conditional formats have to be
/// composed as element trees regardless.
/// </summary>
public class OpenXmlReportBenchmarks
{
    // Stylesheet indices, fixed by the order they are appended in BuildStylesheet().
    private const uint HeaderStyle = 1;
    private const uint DateStyle = 2;
    private const uint PriceStyle = 3;

    private const uint DateNumberFormatId = 164;
    private const uint PriceNumberFormatId = 165;

    private const string ChartNamespace = "http://schemas.openxmlformats.org/drawingml/2006/chart";

    // Arbitrary but stable ids linking the two axes together.
    private const uint CategoryAxisId = 111111111U;
    private const uint ValueAxisId = 222222222U;

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
    internal static bool WriteArtifact() => ReportOutput.Save("openxml", Build);

    private static void Build(Stream output)
    {
        using var doc = SpreadsheetDocument.Create(output, SpreadsheetDocumentType.Workbook);

        var wbPart = doc.AddWorkbookPart();
        wbPart.Workbook = new Workbook();

        var stylesPart = wbPart.AddNewPart<WorkbookStylesPart>();
        stylesPart.Stylesheet = BuildStylesheet();
        stylesPart.Stylesheet.Save();

        var wsPart = wbPart.AddNewPart<WorksheetPart>();
        var worksheet = new Worksheet();

        // Schema order inside <worksheet> is fixed: cols, then sheetData, then
        // conditionalFormatting, then drawing. Appending out of order produces a file Excel
        // refuses to open.
        worksheet.Append(BuildColumns());
        worksheet.Append(BuildSheetData());
        worksheet.Append(BuildConditionalFormatting());

        var drawingsPart = wsPart.AddNewPart<DrawingsPart>();
        var chartPart = drawingsPart.AddNewPart<ChartPart>();
        chartPart.ChartSpace = BuildChartSpace();
        drawingsPart.WorksheetDrawing = BuildWorksheetDrawing(drawingsPart.GetIdOfPart(chartPart));

        worksheet.Append(new Drawing { Id = wsPart.GetIdOfPart(drawingsPart) });
        wsPart.Worksheet = worksheet;

        var sheets = wbPart.Workbook.AppendChild(new Sheets());
        sheets.AppendChild(new Sheet
        {
            Id = wbPart.GetIdOfPart(wsPart),
            SheetId = 1,
            Name = ReportLayout.SheetName,
        });
        wbPart.Workbook.Save();
    }

    // ---- Styles ------------------------------------------------------------------------

    private static Stylesheet BuildStylesheet() =>
        new(
            new NumberingFormats(
                new NumberingFormat
                {
                    NumberFormatId = DateNumberFormatId,
                    FormatCode = ReportLayout.DateFormat,
                },
                new NumberingFormat
                {
                    NumberFormatId = PriceNumberFormatId,
                    FormatCode = ReportLayout.PriceFormat,
                }),
            new Fonts(
                new Font(),
                new Font(new Bold())),
            // Excel requires the first two fills to be None and Gray125, in that order.
            new Fills(
                new Fill(new PatternFill { PatternType = PatternValues.None }),
                new Fill(new PatternFill { PatternType = PatternValues.Gray125 })),
            new Borders(new Border()),
            new CellStyleFormats(new CellFormat()),
            new CellFormats(
                new CellFormat(),
                new CellFormat { FontId = 1, ApplyFont = true },
                new CellFormat { NumberFormatId = DateNumberFormatId, ApplyNumberFormat = true },
                new CellFormat { NumberFormatId = PriceNumberFormatId, ApplyNumberFormat = true }),
            // Differential formats referenced by dxfId from the conditional-format rules.
            new DifferentialFormats(
                Dxf(ReportLayout.UpFill, ReportLayout.UpFont),
                Dxf(ReportLayout.DownFill, ReportLayout.DownFont))
            {
                Count = 2,
            });

    /// <summary>
    /// Builds a differential format for a conditional-format rule.
    /// </summary>
    /// <remarks>
    /// The fill colour goes in <c>bgColor</c>, not <c>fgColor</c>: a dxf inverts the usual
    /// pattern-fill roles, so for a solid conditional-format fill it is the background colour
    /// Excel paints. Every other library in this suite agrees — EPPlus and NPOI emit
    /// <c>patternType="solid"</c> with only a <c>bgColor</c>, and ClosedXML and XLibur emit the
    /// same plus an explicit <c>&lt;fgColor auto="1"/&gt;</c>.
    /// </remarks>
    private static DifferentialFormat Dxf((byte R, byte G, byte B) fill, (byte R, byte G, byte B) font) =>
        new(
            new Font(new Color { Rgb = Argb(font) }),
            new Fill(new PatternFill
            {
                PatternType = PatternValues.Solid,
                BackgroundColor = new BackgroundColor { Rgb = Argb(fill) },
            }));

    private static string Argb((byte R, byte G, byte B) c) => $"FF{ReportLayout.Hex(c)}";

    // ---- Columns -----------------------------------------------------------------------

    /// <summary>
    /// Widens the week-ending column to fit its contents.
    /// </summary>
    /// <remarks>
    /// This is *not* auto-fit. The raw SDK has no font or text-measurement engine, so there is
    /// nothing to ask — the width has to be computed and written explicitly. The estimate comes
    /// from the longest string the column holds; the four libraries that do measure text will
    /// land on slightly different widths.
    /// </remarks>
    private static Columns BuildColumns() =>
        new(new Column
        {
            Min = (uint)ReportLayout.WeekEndingCol,
            Max = (uint)ReportLayout.WeekEndingCol,
            Width = ReportLayout.EstimatedWeekEndingWidth,
            CustomWidth = true,
        });

    // ---- Sheet data --------------------------------------------------------------------

    private static SheetData BuildSheetData()
    {
        var sheetData = new SheetData();

        var header = new Row { RowIndex = (uint)ReportLayout.HeaderRow };
        header.Append(TextCell(ReportLayout.HeaderRow, ReportLayout.WeekNoCol, "Week"));
        header.Append(TextCell(ReportLayout.HeaderRow, ReportLayout.WeekEndingCol, ReportLayout.WeekEndingHeader));
        for (var s = 0; s < StockData.SymbolCount; s++)
            header.Append(TextCell(ReportLayout.HeaderRow, ReportLayout.FirstPriceCol + s, StockData.Symbols[s]));
        sheetData.Append(header);

        for (var w = 0; w < StockData.WeekCount; w++)
        {
            var rowIndex = ReportLayout.FirstDataRow + w;
            var row = new Row { RowIndex = (uint)rowIndex };

            row.Append(NumberCell(rowIndex, ReportLayout.WeekNoCol, w + 1, null));
            row.Append(NumberCell(rowIndex, ReportLayout.WeekEndingCol, StockData.WeekEndings[w].ToOADate(), DateStyle));

            var prices = StockData.Prices[w];
            for (var s = 0; s < prices.Length; s++)
                row.Append(NumberCell(rowIndex, ReportLayout.FirstPriceCol + s, prices[s], PriceStyle));

            sheetData.Append(row);
        }

        return sheetData;
    }

    private static Cell TextCell(int row, int col, string text) =>
        new(new InlineString(new Text(text)))
        {
            CellReference = Ref(row, col),
            DataType = CellValues.InlineString,
            StyleIndex = HeaderStyle,
        };

    private static Cell NumberCell(int row, int col, double value, uint? styleIndex)
    {
        var cell = new Cell(new CellValue(value.ToString("R", CultureInfo.InvariantCulture)))
        {
            CellReference = Ref(row, col),
            DataType = CellValues.Number,
        };
        if (styleIndex.HasValue) cell.StyleIndex = styleIndex.Value;
        return cell;
    }

    private static string Ref(int row, int col) => $"{ReportLayout.ColumnLetter(col)}{row}";

    // ---- Conditional formatting --------------------------------------------------------

    private static ConditionalFormatting BuildConditionalFormatting() =>
        new(
            new ConditionalFormattingRule(new Formula(ReportLayout.UpFormula))
            {
                Type = ConditionalFormatValues.Expression,
                FormatId = 0U,
                Priority = 1,
            },
            new ConditionalFormattingRule(new Formula(ReportLayout.DownFormula))
            {
                Type = ConditionalFormatValues.Expression,
                FormatId = 1U,
                Priority = 2,
            })
        {
            SequenceOfReferences = new ListValue<StringValue> { InnerText = ReportLayout.CfRangeA1 },
        };

    // ---- Drawing (anchors the chart into the sheet) -------------------------------------

    private static Xdr.WorksheetDrawing BuildWorksheetDrawing(string chartRelationshipId)
    {
        var anchor = new Xdr.TwoCellAnchor(
            new Xdr.FromMarker(
                new Xdr.ColumnId((ReportLayout.ChartFirstCol - 1).ToString(CultureInfo.InvariantCulture)),
                new Xdr.ColumnOffset("0"),
                new Xdr.RowId((ReportLayout.ChartFirstRow - 1).ToString(CultureInfo.InvariantCulture)),
                new Xdr.RowOffset("0")),
            new Xdr.ToMarker(
                new Xdr.ColumnId((ReportLayout.ChartFirstCol - 1 + ReportLayout.ChartColSpan).ToString(CultureInfo.InvariantCulture)),
                new Xdr.ColumnOffset("0"),
                new Xdr.RowId((ReportLayout.ChartFirstRow - 1 + ReportLayout.ChartRowSpan).ToString(CultureInfo.InvariantCulture)),
                new Xdr.RowOffset("0")),
            new Xdr.GraphicFrame(
                new Xdr.NonVisualGraphicFrameProperties(
                    new Xdr.NonVisualDrawingProperties { Id = 2U, Name = "Weekly prices chart" },
                    new Xdr.NonVisualGraphicFrameDrawingProperties()),
                new Xdr.Transform(
                    new A.Offset { X = 0L, Y = 0L },
                    new A.Extents { Cx = 0L, Cy = 0L }),
                new A.Graphic(
                    new A.GraphicData(new C.ChartReference { Id = chartRelationshipId })
                    {
                        Uri = ChartNamespace,
                    })),
            new Xdr.ClientData());

        return new Xdr.WorksheetDrawing(anchor);
    }

    // ---- Chart part --------------------------------------------------------------------

    private static C.ChartSpace BuildChartSpace()
    {
        var lineChart = new C.LineChart(
            new C.Grouping { Val = C.GroupingValues.Standard },
            new C.VaryColors { Val = false });

        for (var s = 0; s < StockData.SymbolCount; s++)
            lineChart.Append(BuildSeries(s));

        lineChart.Append(new C.ShowMarker { Val = false });
        lineChart.Append(new C.AxisId { Val = CategoryAxisId });
        lineChart.Append(new C.AxisId { Val = ValueAxisId });

        var plotArea = new C.PlotArea(
            new C.Layout(),
            lineChart,
            BuildCategoryAxis(),
            BuildValueAxis());

        var chart = new C.Chart(
            new C.Title(
                new C.ChartText(
                    new C.RichText(
                        new A.BodyProperties(),
                        new A.ListStyle(),
                        new A.Paragraph(new A.Run(new A.Text(ReportLayout.ChartTitle)))))),
            new C.AutoTitleDeleted { Val = false },
            plotArea,
            new C.Legend(new C.LegendPosition { Val = C.LegendPositionValues.Right }),
            new C.PlotVisibleOnly { Val = true });

        return new C.ChartSpace(new C.EditingLanguage { Val = "en-US" }, chart);
    }

    private static C.LineChartSeries BuildSeries(int symbolIndex)
    {
        var priceColumn = ReportLayout.ColumnLetter(ReportLayout.FirstPriceCol + symbolIndex);

        return new C.LineChartSeries(
            new C.Index { Val = (uint)symbolIndex },
            new C.Order { Val = (uint)symbolIndex },
            // Series name reads from the header cell, so the legend picks up the ticker.
            new C.SeriesText(new C.StringReference
            {
                Formula = new C.Formula($"'{ReportLayout.SheetName}'!${priceColumn}${ReportLayout.HeaderRow}"),
            }),
            // Week-ending dates are numeric cells, so the category axis is a number reference
            // carrying the date format code rather than a string reference.
            new C.CategoryAxisData(new C.NumberReference
            {
                Formula = new C.Formula(ReportLayout.CategoryRef()),
                NumberingCache = new C.NumberingCache(new C.FormatCode(ReportLayout.DateFormat)),
            }),
            new C.Values(new C.NumberReference
            {
                Formula = new C.Formula(ReportLayout.PriceColumnRef(symbolIndex)),
            }),
            new C.Smooth { Val = false });
    }

    private static C.CategoryAxis BuildCategoryAxis() =>
        new(
            new C.AxisId { Val = CategoryAxisId },
            new C.Scaling(new C.Orientation { Val = C.OrientationValues.MinMax }),
            new C.Delete { Val = false },
            new C.AxisPosition { Val = C.AxisPositionValues.Bottom },
            new C.NumberingFormat { FormatCode = ReportLayout.DateFormat, SourceLinked = false },
            new C.TickLabelPosition { Val = C.TickLabelPositionValues.NextTo },
            new C.CrossingAxis { Val = ValueAxisId },
            new C.Crosses { Val = C.CrossesValues.AutoZero },
            new C.AutoLabeled { Val = true },
            new C.LabelAlignment { Val = C.LabelAlignmentValues.Center },
            new C.LabelOffset { Val = 100 },
            new C.NoMultiLevelLabels { Val = false });

    private static C.ValueAxis BuildValueAxis() =>
        new(
            new C.AxisId { Val = ValueAxisId },
            new C.Scaling(new C.Orientation { Val = C.OrientationValues.MinMax }),
            new C.Delete { Val = false },
            new C.AxisPosition { Val = C.AxisPositionValues.Left },
            new C.MajorGridlines(),
            new C.NumberingFormat { FormatCode = ReportLayout.PriceFormat, SourceLinked = false },
            new C.TickLabelPosition { Val = C.TickLabelPositionValues.NextTo },
            new C.CrossingAxis { Val = CategoryAxisId },
            new C.Crosses { Val = C.CrossesValues.AutoZero },
            new C.CrossBetween { Val = C.CrossBetweenValues.Between });
}
