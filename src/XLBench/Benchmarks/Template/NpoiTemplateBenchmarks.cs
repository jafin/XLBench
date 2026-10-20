using BenchmarkDotNet.Attributes;
using NPOI.SS;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using XLBench.Data;

namespace XLBench.Benchmarks.Template;

/// <summary>
/// NPOI fill-template-and-save: full scenario support.
///
/// A defined name is a formula, not a range — <c>GetName(name).RefersToFormula</c> is the text
/// <c>Report!$A$1</c> — so each named cell is reached by parsing that into a
/// <see cref="CellReference"/> and walking to the row and cell. The table is grown by assigning
/// <c>XSSFTable.CellReferences</c>, which rewrites the table's <c>ref</c> and its auto-filter
/// range together.
///
/// <para>The template's chart is not touched, but NPOI parses it into its own chart model on load
/// and serializes that back out on save. The title comes back as <c>&lt;a:rich&gt;</c> where the
/// schema requires <c>&lt;c:rich&gt;</c> — the same <c>CT_Tx</c> prefix bug the report scenario
/// works around — along with nine further schema errors in the chart part. Excel still opens the
/// file.</para>
/// </summary>
public class NpoiTemplateBenchmarks
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
        TemplateOutput.Save("npoi", output => Fill(new MemoryStream(TemplateData.TemplateXlsx), output));

    private static void Fill(Stream input, Stream output)
    {
        using var wb = new XSSFWorkbook(input);
        var sheet = (XSSFSheet)wb.GetSheet(TemplateData.SheetName);

        NamedCell(wb, sheet, TemplateData.TitleName).SetCellValue(TemplateData.Title);
        NamedCell(wb, sheet, TemplateData.DateName).SetCellValue(TemplateData.ReportDate);
        NamedCell(wb, sheet, TemplateData.RegionName).SetCellValue(TemplateData.Region);

        // NPOI rows and cells are 0-based; TemplateData's are 1-based.
        var rows = TemplateData.Rows;
        for (var i = 0; i < rows.Length; i++)
        {
            var r = TemplateData.FirstDataRow + i;
            var line = rows[i];
            var row = sheet.GetRow(r - 1) ?? sheet.CreateRow(r - 1);
            Cell(row, TemplateData.DateCol).SetCellValue(line.Date);
            Cell(row, TemplateData.ProductCol).SetCellValue(line.Product);
            Cell(row, TemplateData.CategoryCol).SetCellValue(line.Category);
            Cell(row, TemplateData.QuantityCol).SetCellValue(line.Quantity);
            Cell(row, TemplateData.UnitPriceCol).SetCellValue(line.UnitPrice);
            Cell(row, TemplateData.AmountCol).SetCellFormula(TemplateData.AmountFormula(r));
        }

        wb.GetTable(TemplateData.TableName).CellReferences =
            new AreaReference(TemplateData.TableRef, SpreadsheetVersion.EXCEL2007);

        wb.Write(output, leaveOpen: true);
    }

    private static XSSFCell NamedCell(XSSFWorkbook wb, XSSFSheet sheet, string name)
    {
        var reference = new CellReference(wb.GetName(name).RefersToFormula);
        var row = sheet.GetRow(reference.Row) ?? sheet.CreateRow(reference.Row);
        return (XSSFCell)(row.GetCell(reference.Col) ?? row.CreateCell(reference.Col));
    }

    private static NPOI.SS.UserModel.ICell Cell(NPOI.SS.UserModel.IRow row, int column) =>
        row.GetCell(column - 1) ?? row.CreateCell(column - 1);
}
