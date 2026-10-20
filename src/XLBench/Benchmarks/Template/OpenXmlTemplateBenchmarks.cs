using System.Globalization;
using BenchmarkDotNet.Attributes;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using XLBench.Data;

namespace XLBench.Benchmarks.Template;

/// <summary>
/// OpenXML SDK fill-template-and-save: the whole scenario by hand, because the SDK is a
/// document-object model over the package and nothing more.
///
/// <para>Each named cell is found by reading the defined name's formula text and walking
/// <c>sheetData</c> to it. The rows are spliced into <c>sheetData</c> in order — the template's
/// rows 6–11 already hold the summary block in H:J, so the new cells go in ahead of those, and
/// every later row is appended. The table is grown by rewriting its <c>ref</c> and its
/// auto-filter's <c>ref</c> in the table part.</para>
///
/// <para>Two things the libraries do for themselves are done explicitly here. A new cell is given
/// its column's style index from <c>&lt;cols&gt;</c>: Excel applies a column style only to cells
/// that do not exist, so a written cell without <c>s</c> would show its date as a serial number.
/// And <c>fullCalcOnLoad</c> is set, because the summary block's cached SUMIFS results still
/// describe the empty template; without a calculation engine there is nothing else to correct
/// them. It is the only library here whose package is never parsed into a model of its own,
/// which is what its timing reflects.</para>
/// </summary>
public class OpenXmlTemplateBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        TemplateData.EnsureLoaded();
        _bytes = TemplateData.TemplateXlsx;
    }

    /// <inheritdoc cref="ClosedXmlTemplateBenchmarks.FillTemplateAndSave"/>
    [Benchmark]
    public long FillTemplateAndSave()
    {
        using var output = new MemoryStream();
        Fill(new MemoryStream(_bytes), output);
        return output.Length;
    }

    [GlobalCleanup]
    public void SaveArtifact() => WriteArtifact();

    /// <inheritdoc cref="ClosedXmlTemplateBenchmarks.WriteArtifact"/>
    internal static bool WriteArtifact() =>
        TemplateOutput.Save("openxml", output => Fill(new MemoryStream(TemplateData.TemplateXlsx), output));

    private static void Fill(Stream input, Stream output)
    {
        // The SDK edits the package in place, so it needs a stream it can grow. MemoryStream over
        // a byte[] is fixed-length.
        using var package = new MemoryStream();
        input.CopyTo(package);

        using (var doc = SpreadsheetDocument.Open(package, isEditable: true))
        {
            var wbPart = doc.WorkbookPart!;
            var workbook = wbPart.Workbook!;
            var sheet = workbook.Sheets!.Elements<Sheet>().First(s => s.Name == TemplateData.SheetName);
            var wsPart = (WorksheetPart)wbPart.GetPartById(sheet.Id!);
            var worksheet = wsPart.Worksheet!;
            var sheetData = worksheet.GetFirstChild<SheetData>()!;

            var rowsByIndex = sheetData.Elements<Row>().ToDictionary(r => (int)r.RowIndex!.Value);
            var columnStyles = ColumnStyles(worksheet);

            var names = workbook.DefinedNames!.Elements<DefinedName>().ToDictionary(n => n.Name!.Value!, n => n.Text);
            SetString(CellAt(sheetData, rowsByIndex, names[TemplateData.TitleName]), TemplateData.Title);
            SetNumber(CellAt(sheetData, rowsByIndex, names[TemplateData.DateName]), TemplateData.ReportDate.ToOADate());
            SetString(CellAt(sheetData, rowsByIndex, names[TemplateData.RegionName]), TemplateData.Region);

            var rows = TemplateData.Rows;
            for (var i = 0; i < rows.Length; i++)
            {
                var r = TemplateData.FirstDataRow + i;
                var line = rows[i];

                Cell[] cells =
                [
                    NumberCell(r, TemplateData.DateCol, line.Date.ToOADate(), columnStyles),
                    StringCell(r, TemplateData.ProductCol, line.Product, columnStyles),
                    StringCell(r, TemplateData.CategoryCol, line.Category, columnStyles),
                    NumberCell(r, TemplateData.QuantityCol, line.Quantity, columnStyles),
                    NumberCell(r, TemplateData.UnitPriceCol, line.UnitPrice, columnStyles),
                    FormulaCell(r, TemplateData.AmountCol, TemplateData.AmountFormula(r), columnStyles),
                ];

                if (rowsByIndex.TryGetValue(r, out var existing))
                {
                    // Rows 6–11 already carry the summary block in H:J. Cells must stay in column
                    // order, and A:F all sort before H, so the new cells go in ahead of them.
                    var first = existing.FirstChild;
                    foreach (var cell in cells)
                    {
                        if (first is null) existing.Append(cell);
                        else existing.InsertBefore(cell, first);
                    }
                }
                else
                {
                    // Every row past the template's last one is new, and rows arrive in order.
                    sheetData.Append(new Row(cells) { RowIndex = (uint)r });
                }
            }

            if (worksheet.GetFirstChild<SheetDimension>() is { } dimension)
                dimension.Reference = $"A1:J{TemplateData.LastDataRow}";

            var tablePart = wsPart.TableDefinitionParts.First(t => t.Table!.Name == TemplateData.TableName);
            tablePart.Table!.Reference = TemplateData.TableRef;
            if (tablePart.Table.AutoFilter is { } autoFilter)
                autoFilter.Reference = TemplateData.TableRef;

            workbook.CalculationProperties ??= new CalculationProperties();
            workbook.CalculationProperties.FullCalculationOnLoad = true;
        }

        package.Position = 0;
        package.CopyTo(output);
    }

    /// <summary>Column style index by 1-based column, from the worksheet's <c>&lt;cols&gt;</c>.</summary>
    private static Dictionary<int, uint> ColumnStyles(Worksheet worksheet)
    {
        var styles = new Dictionary<int, uint>();
        foreach (var column in worksheet.GetFirstChild<Columns>()?.Elements<Column>() ?? [])
        {
            if (column.Style?.Value is not { } style) continue;
            for (var c = (int)column.Min!.Value; c <= (int)column.Max!.Value; c++)
                styles[c] = style;
        }
        return styles;
    }

    /// <summary>The existing cell a defined name points at. The template's named cells all exist.</summary>
    private static Cell CellAt(SheetData sheetData, Dictionary<int, Row> rows, string reference)
    {
        var (row, col) = TemplateData.ParseCell(reference);
        var address = $"{A1.ColumnLetter(col)}{row}";
        return rows[row].Elements<Cell>().First(c => c.CellReference == address);
    }

    private static void SetString(Cell cell, string text)
    {
        cell.CellValue = null;
        cell.DataType = CellValues.InlineString;
        cell.InlineString = new InlineString(new Text(text));
    }

    private static void SetNumber(Cell cell, double value)
    {
        cell.DataType = null;
        cell.CellValue = new CellValue(value.ToString("R", CultureInfo.InvariantCulture));
    }

    private static Cell StringCell(int row, int col, string text, Dictionary<int, uint> styles) =>
        Styled(new Cell
        {
            CellReference = $"{A1.ColumnLetter(col)}{row}",
            DataType = CellValues.InlineString,
            InlineString = new InlineString(new Text(text)),
        }, col, styles);

    private static Cell NumberCell(int row, int col, double value, Dictionary<int, uint> styles) =>
        Styled(new Cell
        {
            CellReference = $"{A1.ColumnLetter(col)}{row}",
            CellValue = new CellValue(value.ToString("R", CultureInfo.InvariantCulture)),
        }, col, styles);

    private static Cell FormulaCell(int row, int col, string formula, Dictionary<int, uint> styles) =>
        Styled(new Cell
        {
            CellReference = $"{A1.ColumnLetter(col)}{row}",
            CellFormula = new CellFormula(formula),
        }, col, styles);

    private static Cell Styled(Cell cell, int col, Dictionary<int, uint> styles)
    {
        if (styles.TryGetValue(col, out var style)) cell.StyleIndex = style;
        return cell;
    }
}
