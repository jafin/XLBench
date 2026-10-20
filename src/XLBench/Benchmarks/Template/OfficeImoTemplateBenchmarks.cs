using BenchmarkDotNet.Attributes;
using OfficeIMO.Excel;
using XLBench.Data;

namespace XLBench.Benchmarks.Template;

/// <summary>
/// OfficeIMO fill-template-and-save: full scenario support.
///
/// <c>ExcelDocument.GetNamedRange(name)</c> resolves a defined name to its A1 text, which is
/// parsed back into a row and column for <c>CellValue</c>. <c>ExcelSheet.ResizeTable</c> grows
/// the table over the written rows while keeping its column names.
///
/// <para>New cells do not take their column's style. A <c>DateTime</c> still comes out as a date,
/// because <c>CellValue</c> applies a built-in date format of its own, but the prices and amounts
/// lose the template's currency format and show as plain numbers.</para>
/// </summary>
public class OfficeImoTemplateBenchmarks
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
        TemplateOutput.Save("officeimo", output => Fill(new MemoryStream(TemplateData.TemplateXlsx), output));

    private static void Fill(Stream input, Stream output)
    {
        using var doc = ExcelDocument.Load(input);
        var ws = doc.Sheets.First(s => s.Name == TemplateData.SheetName);

        var (titleRow, titleCol) = TemplateData.ParseCell(doc.GetNamedRange(TemplateData.TitleName)!);
        ws.CellValue(titleRow, titleCol, TemplateData.Title);
        var (dateRow, dateCol) = TemplateData.ParseCell(doc.GetNamedRange(TemplateData.DateName)!);
        ws.CellValue(dateRow, dateCol, TemplateData.ReportDate);
        var (regionRow, regionCol) = TemplateData.ParseCell(doc.GetNamedRange(TemplateData.RegionName)!);
        ws.CellValue(regionRow, regionCol, TemplateData.Region);

        var rows = TemplateData.Rows;
        for (var i = 0; i < rows.Length; i++)
        {
            var r = TemplateData.FirstDataRow + i;
            var line = rows[i];
            ws.CellValue(r, TemplateData.DateCol, line.Date);
            ws.CellValue(r, TemplateData.ProductCol, line.Product);
            ws.CellValue(r, TemplateData.CategoryCol, line.Category);
            ws.CellValue(r, TemplateData.QuantityCol, line.Quantity);
            ws.CellValue(r, TemplateData.UnitPriceCol, line.UnitPrice);
            ws.CellFormula(r, TemplateData.AmountCol, TemplateData.AmountFormula(r));
        }

        ws.ResizeTable(TemplateData.TableName, TemplateData.TableRef);

        doc.Save(output);
    }
}
