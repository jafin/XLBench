using BenchmarkDotNet.Attributes;
using IronXL;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.Template;

/// <summary>
/// IronXL fill-template-and-save.
///
/// <c>WorkSheet.FindNamedRange</c> resolves a defined name to its address text, which is parsed
/// back into a row and column for <c>SetCellValue</c>.
///
/// <para>IronXL can read a named table (<c>GetNamedTable</c>) and add one
/// (<c>AddNamedTable</c>), but has no call that resizes an existing one, so the table is left at
/// the template's <c>A5:F6</c> and <see cref="TemplateArtifacts"/> will report it. Not yet run
/// against a licence key; the artifact check is the first thing to look at when one is
/// available.</para>
/// </summary>
public class IronXlTemplateBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        IronXlLicense.Ensure();
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
        IronXlLicense.Ensure();
        return TemplateOutput.Save("ironxl", output => Fill(new MemoryStream(TemplateData.TemplateXlsx), output));
    }

    private static void Fill(Stream input, Stream output)
    {
        // WorkBook is not IDisposable — see IronXlReadBenchmarks.
        var wb = WorkBook.Load(input);
        var ws = wb.GetWorkSheet(TemplateData.SheetName);

        // IronXL row and column numbers are 0-based; TemplateData's are 1-based.
        var (titleRow, titleCol) = TemplateData.ParseCell(ws.FindNamedRange(TemplateData.TitleName));
        ws.SetCellValue(titleRow - 1, titleCol - 1, TemplateData.Title);
        var (dateRow, dateCol) = TemplateData.ParseCell(ws.FindNamedRange(TemplateData.DateName));
        ws.SetCellValue(dateRow - 1, dateCol - 1, TemplateData.ReportDate);
        var (regionRow, regionCol) = TemplateData.ParseCell(ws.FindNamedRange(TemplateData.RegionName));
        ws.SetCellValue(regionRow - 1, regionCol - 1, TemplateData.Region);

        var rows = TemplateData.Rows;
        for (var i = 0; i < rows.Length; i++)
        {
            var r = TemplateData.FirstDataRow + i - 1;
            var line = rows[i];
            ws.SetCellValue(r, TemplateData.DateCol - 1, line.Date);
            ws.SetCellValue(r, TemplateData.ProductCol - 1, line.Product);
            ws.SetCellValue(r, TemplateData.CategoryCol - 1, line.Category);
            ws.SetCellValue(r, TemplateData.QuantityCol - 1, line.Quantity);
            ws.SetCellValue(r, TemplateData.UnitPriceCol - 1, line.UnitPrice);
            ws[$"F{r + 1}"].Formula = "=" + TemplateData.AmountFormula(r + 1);
        }

        // No SaveAs(Stream) overload — ToStream() materializes the workbook itself.
        using var ms = wb.ToStream();
        ms.CopyTo(output);
    }
}
