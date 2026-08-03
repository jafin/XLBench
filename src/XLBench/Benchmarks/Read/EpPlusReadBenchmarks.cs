using BenchmarkDotNet.Attributes;
using OfficeOpenXml;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.Read;

public class EpPlusReadBenchmarks
{
    private byte[] _bytes = null!;
    private byte[] _propertiesBytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        EpPlusLicense.Ensure();
        _bytes = TestData.ReadXlsx;
        PropertiesData.EnsureLoaded();
        _propertiesBytes = PropertiesData.SourceXlsx;
    }

    /// <inheritdoc cref="ClosedXmlReadBenchmarks.OpenAmendPropertiesAndSave"/>
    [Benchmark]
    public long OpenAmendPropertiesAndSave()
    {
        using var pkg = new ExcelPackage(new MemoryStream(_propertiesBytes));

        pkg.Workbook.Properties.Title = PropertiesData.Title;
        pkg.Workbook.Properties.Category = PropertiesData.Category;

        using var output = new MemoryStream();
        pkg.SaveAs(output);
        return output.Length;
    }

    [Benchmark]
    public long OpenAndReadAll()
    {
        using var pkg = new ExcelPackage(new MemoryStream(_bytes));
        var ws = pkg.Workbook.Worksheets.First();

        // Iterate the populated cells of the used range (EPPlus's ws.Cells enumerates
        // only existing cells) rather than random indexer access.
        long checksum = 0;
        foreach (var cell in ws.Cells)
            checksum += (cell.GetValue<string>() ?? string.Empty).Length;
        return checksum;
    }
}
