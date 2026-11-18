using BenchmarkDotNet.Attributes;
using OfficeIMO.Excel;
using XLBench.Data;

namespace XLBench.Benchmarks.Template;

/// <summary>
/// OfficeIMO fill-template-and-save: full scenario support.
///
/// <c>ExcelDocument.GetNamedRange(name)</c> resolves a defined name to its range, and the value
/// goes into the range's first cell. <c>ExcelSheet.ResizeTable</c> grows the table over the written
/// rows while keeping its column names.
///
/// <para>The new rows keep the template's date and currency formats.</para>
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

        doc.GetNamedRange(TemplateData.TitleName)!.FirstCell.SetValue(TemplateData.Title);
        doc.GetNamedRange(TemplateData.DateName)!.FirstCell.SetValue(TemplateData.ReportDate);
        doc.GetNamedRange(TemplateData.RegionName)!.FirstCell.SetValue(TemplateData.Region);

        var rows = TemplateData.Rows;
        for (var i = 0; i < rows.Length; i++)
        {
            var r = TemplateData.FirstDataRow + i;
            var line = rows[i];
            ws.Cell(r, TemplateData.DateCol).SetValue(line.Date);
            ws.Cell(r, TemplateData.ProductCol).SetValue(line.Product);
            ws.Cell(r, TemplateData.CategoryCol).SetValue(line.Category);
            ws.Cell(r, TemplateData.QuantityCol).SetValue(line.Quantity);
            ws.Cell(r, TemplateData.UnitPriceCol).SetValue(line.UnitPrice);
            ws.Cell(r, TemplateData.AmountCol).SetFormula(TemplateData.AmountFormula(r));
        }

        ws.ResizeTable(TemplateData.TableName, TemplateData.TableRef);

        doc.Save(output);
    }
}
