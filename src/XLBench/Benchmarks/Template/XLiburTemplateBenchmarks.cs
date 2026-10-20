using BenchmarkDotNet.Attributes;
using XLBench.Data;
using XLibur.Excel;

namespace XLBench.Benchmarks.Template;

/// <summary>
/// XLibur fill-template-and-save: full scenario support.
///
/// The API is ClosedXML-shaped, so this mirrors <see cref="ClosedXmlTemplateBenchmarks"/> line
/// for line — <c>Range(name)</c> for the named cells, <c>IXLTable.Resize</c> for the table.
/// </summary>
public class XLiburTemplateBenchmarks
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
        TemplateOutput.Save("xlibur", output => Fill(new MemoryStream(TemplateData.TemplateXlsx), output));

    private static void Fill(Stream input, Stream output)
    {
        using var wb = new XLWorkbook(input);

        wb.Range(TemplateData.TitleName)!.FirstCell().Value = TemplateData.Title;
        wb.Range(TemplateData.DateName)!.FirstCell().Value = TemplateData.ReportDate;
        wb.Range(TemplateData.RegionName)!.FirstCell().Value = TemplateData.Region;

        var ws = wb.Worksheet(TemplateData.SheetName);
        var rows = TemplateData.Rows;
        for (var i = 0; i < rows.Length; i++)
        {
            var r = TemplateData.FirstDataRow + i;
            var line = rows[i];
            ws.Cell(r, TemplateData.DateCol).Value = line.Date;
            ws.Cell(r, TemplateData.ProductCol).Value = line.Product;
            ws.Cell(r, TemplateData.CategoryCol).Value = line.Category;
            ws.Cell(r, TemplateData.QuantityCol).Value = line.Quantity;
            ws.Cell(r, TemplateData.UnitPriceCol).Value = line.UnitPrice;
            ws.Cell(r, TemplateData.AmountCol).FormulaA1 = TemplateData.AmountFormula(r);
        }

        ws.Table(TemplateData.TableName).Resize(ws.Range(TemplateData.TableRef));

        wb.SaveAs(output);
    }
}
