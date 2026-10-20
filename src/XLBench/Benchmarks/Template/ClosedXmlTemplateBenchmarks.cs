using BenchmarkDotNet.Attributes;
using ClosedXML.Excel;
using XLBench.Data;

namespace XLBench.Benchmarks.Template;

/// <summary>
/// ClosedXML fill-template-and-save: full scenario support through the high-level cell model.
///
/// The named cells resolve with <c>Range(name)</c>, and <c>IXLTable.Resize</c> grows the table
/// over the rows written beneath its header. New cells take their column's style, so the dates
/// and prices come out formatted without being told to.
///
/// <para>ClosedXML has no public chart API (see the report scenario), but it does not need one
/// here: the template's chart part is carried through the save untouched, still plotting the
/// summary block.</para>
/// </summary>
public class ClosedXmlTemplateBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        TemplateData.EnsureLoaded();
        _bytes = TemplateData.TemplateXlsx;
    }

    /// <summary>
    /// Opens the Excel-authored template, fills its named cells and table, and saves it. Returns
    /// the serialized length so the save cannot be optimized away; unlike the edit scenarios, the
    /// save is the point here and sits inside the measured region.
    /// </summary>
    [Benchmark]
    public long FillTemplateAndSave()
    {
        using var output = new MemoryStream();
        Fill(new MemoryStream(_bytes), output);
        return output.Length;
    }

    [GlobalCleanup]
    public void SaveArtifact() => WriteArtifact();

    /// <summary>Runs one unmeasured fill and writes it to <c>output/</c>.</summary>
    /// <returns><c>true</c> if the file was written; <c>false</c> if it was locked and skipped.</returns>
    internal static bool WriteArtifact() =>
        TemplateOutput.Save("closedxml", output => Fill(new MemoryStream(TemplateData.TemplateXlsx), output));

    private static void Fill(Stream input, Stream output)
    {
        using var wb = new XLWorkbook(input);

        wb.Range(TemplateData.TitleName).FirstCell().Value = TemplateData.Title;
        wb.Range(TemplateData.DateName).FirstCell().Value = TemplateData.ReportDate;
        wb.Range(TemplateData.RegionName).FirstCell().Value = TemplateData.Region;

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
