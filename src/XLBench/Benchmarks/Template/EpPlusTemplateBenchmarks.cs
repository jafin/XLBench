using BenchmarkDotNet.Attributes;
using OfficeOpenXml;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.Template;

/// <summary>
/// EPPlus fill-template-and-save: full scenario support.
///
/// The named cells are <c>ExcelNamedRange</c>s, which are ranges in their own right, so a value
/// is written straight to <c>Workbook.Names[name]</c>. The table grows with
/// <c>ExcelTable.AddRow(n)</c>, which extends it downwards before the rows are written.
///
/// <para><c>AddRow</c> makes room by inserting cells below the table, within the table's own
/// columns only — the summary block beside it stays put. Like an insert in Excel, that also
/// stretches every range spanning the insertion point, so the template's validation
/// (<c>C6:C5000</c>) and conditional formatting (<c>F6:F5000</c>) come back as
/// <c>C6:C6999</c> and <c>F6:F6999</c>. Both still cover every row; nothing is lost.</para>
/// </summary>
public class EpPlusTemplateBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        EpPlusLicense.Ensure();
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
    internal static bool WriteArtifact()
    {
        EpPlusLicense.Ensure();
        return TemplateOutput.Save("epplus", output => Fill(new MemoryStream(TemplateData.TemplateXlsx), output));
    }

    private static void Fill(Stream input, Stream output)
    {
        using var pkg = new ExcelPackage(input);
        var wb = pkg.Workbook;

        wb.Names[TemplateData.TitleName].Value = TemplateData.Title;
        wb.Names[TemplateData.DateName].Value = TemplateData.ReportDate;
        wb.Names[TemplateData.RegionName].Value = TemplateData.Region;

        var ws = wb.Worksheets[TemplateData.SheetName];

        // The template ships one empty data row, so the table needs RowCount - 1 more.
        ws.Tables[TemplateData.TableName].AddRow(TemplateData.RowCount - 1);

        var rows = TemplateData.Rows;
        for (var i = 0; i < rows.Length; i++)
        {
            var r = TemplateData.FirstDataRow + i;
            var line = rows[i];
            ws.Cells[r, TemplateData.DateCol].Value = line.Date;
            ws.Cells[r, TemplateData.ProductCol].Value = line.Product;
            ws.Cells[r, TemplateData.CategoryCol].Value = line.Category;
            ws.Cells[r, TemplateData.QuantityCol].Value = line.Quantity;
            ws.Cells[r, TemplateData.UnitPriceCol].Value = line.UnitPrice;
            ws.Cells[r, TemplateData.AmountCol].Formula = TemplateData.AmountFormula(r);
        }

        pkg.SaveAs(output);
    }
}
