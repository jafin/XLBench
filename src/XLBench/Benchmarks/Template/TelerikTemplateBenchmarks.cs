using BenchmarkDotNet.Attributes;
using Telerik.Documents.Spreadsheet.FormatProviders.OpenXml.Xlsx;
using Telerik.Documents.Spreadsheet.Model;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.Template;

/// <summary>
/// RadSpreadProcessing fill-template-and-save: the fill runs, but the template does not survive
/// it intact.
///
/// The named cells resolve through <c>Workbook.Names</c>, whose <c>RefersTo</c> is the name's
/// formula text. The rows are written with <c>SetValue</c> like any other cell.
///
/// <para>RadSpreadProcessing has no Excel-table model, so there is no table to grow: the
/// <c>Sales</c> table is dropped on import. With it go the structured references the summary
/// block depends on. Each <c>SUMIFS(Sales[Amount],…)</c> cannot be parsed, and is imported as a
/// text cell that reads like the formula, so the totals and the chart plotting them are blank in
/// the saved file. The chart, validation, conditional formatting, merged title and named styles
/// all survive.</para>
/// </summary>
public class TelerikTemplateBenchmarks
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
        TemplateOutput.Save("telerik", output => Fill(new MemoryStream(TemplateData.TemplateXlsx), output));

    private static void Fill(Stream input, Stream output)
    {
        var provider = new XlsxFormatProvider();
        using var workbook = provider.Import(input, null).WithoutHistory();
        var worksheet = workbook.Worksheets.GetByName(TemplateData.SheetName)!;

        NamedCell(workbook, worksheet, TemplateData.TitleName).SetValue(TemplateData.Title);
        NamedCell(workbook, worksheet, TemplateData.DateName).SetValue(TemplateData.ReportDate);
        NamedCell(workbook, worksheet, TemplateData.RegionName).SetValue(TemplateData.Region);

        // Telerik rows and columns are 0-based; TemplateData's are 1-based.
        var rows = TemplateData.Rows;
        for (var i = 0; i < rows.Length; i++)
        {
            var r = TemplateData.FirstDataRow + i - 1;
            var line = rows[i];
            worksheet.Cells[r, TemplateData.DateCol - 1].SetValue(line.Date);
            worksheet.Cells[r, TemplateData.ProductCol - 1].SetValue(line.Product);
            worksheet.Cells[r, TemplateData.CategoryCol - 1].SetValue(line.Category);
            worksheet.Cells[r, TemplateData.QuantityCol - 1].SetValue(line.Quantity);
            worksheet.Cells[r, TemplateData.UnitPriceCol - 1].SetValue(line.UnitPrice);
            worksheet.Cells[r, TemplateData.AmountCol - 1].SetValue("=" + TemplateData.AmountFormula(r + 1));
        }

        provider.Export(workbook, output, null);
    }

    private static CellSelection NamedCell(Workbook workbook, Worksheet worksheet, string name)
    {
        if (!workbook.Names.TryGetSpreadsheetName(name, out var spreadsheetName))
            throw new InvalidOperationException($"The template has no defined name '{name}'.");

        var (row, col) = TemplateData.ParseCell(spreadsheetName.RefersTo);
        return worksheet.Cells[row - 1, col - 1];
    }
}
