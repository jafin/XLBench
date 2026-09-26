using BenchmarkDotNet.Attributes;
using OfficeIMO.Excel;
using XLBench.Data;

namespace XLBench.Benchmarks.Read;

/// <summary>
/// Both benchmarks open the workbook as an editable <c>ExcelDocument</c>, like the other
/// object-model libraries. <see cref="OpenAndReadAll"/> walks the sheet with
/// <c>ExcelSheet.EnumerateCells()</c>, which streams the non-empty cells without probing every
/// coordinate in the used range — the counterpart of ClosedXML's <c>CellsUsed()</c>.
///
/// <para>OfficeIMO also has a forward-only <c>ExcelDocument.OpenDataReader</c>, closer in kind to
/// MiniExcel's reader. It is not used here because it answers a different question: rows through
/// an <c>IDataReader</c>, not the cells of an opened workbook.</para>
/// </summary>
public class OfficeImoReadBenchmarks
{
    private byte[] _bytes = null!;
    private byte[] _propertiesBytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        _bytes = TestData.ReadXlsx;
        PropertiesData.EnsureLoaded();
        _propertiesBytes = PropertiesData.SourceXlsx;
    }

    /// <inheritdoc cref="Benchmarks.Read.ClosedXmlReadBenchmarks.OpenAmendPropertiesAndSave"/>
    [Benchmark]
    public long OpenAmendPropertiesAndSave()
    {
        using var doc = ExcelDocument.Load(new MemoryStream(_propertiesBytes));

        doc.BuiltinDocumentProperties.Title = PropertiesData.Title;
        doc.BuiltinDocumentProperties.Category = PropertiesData.Category;

        using var output = new MemoryStream();
        doc.Save(output);
        return output.Length;
    }

    [Benchmark]
    public long OpenAndReadAll()
    {
        using var doc = ExcelDocument.Load(new MemoryStream(_bytes));
        var ws = doc.Sheets[0];

        long checksum = 0;
        foreach (var cell in ws.EnumerateCells())
            checksum += (cell.Value?.ToString() ?? string.Empty).Length;
        return checksum;
    }
}
