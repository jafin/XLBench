using BenchmarkDotNet.Attributes;
using XLBench.Data;
using XLibur.Excel;

namespace XLBench.Benchmarks.Read;

public class XLiburReadBenchmarks
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
        using var wb = new XLWorkbook(new MemoryStream(_propertiesBytes));

        wb.Properties.Title = PropertiesData.Title;
        wb.Properties.Category = PropertiesData.Category;

        using var output = new MemoryStream();
        wb.SaveAs(output);
        return output.Length;
    }

    [Benchmark]
    public long OpenAndReadAll()
    {
        using var wb = new XLWorkbook(new MemoryStream(_bytes));
        var ws = wb.Worksheets.First();

        // Idiomatic iteration over populated cells (mirrors the ClosedXML benchmark).
        long checksum = 0;
        foreach (var cell in ws.CellsUsed())
            checksum += cell.GetString().Length;
        return checksum;
    }
}
