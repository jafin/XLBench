using BenchmarkDotNet.Attributes;
using ClosedXML.Excel;
using XLBench.Data;

namespace XLBench.Benchmarks.Read;

public class ClosedXmlReadBenchmarks
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

    /// <summary>
    /// Round trip: open the workbook, set its Title and Category, save it back out. Returns the
    /// serialized length so the save cannot be optimized away.
    /// </summary>
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

        // Idiomatic iteration over populated cells (random Cell(row,col) access is
        // pathologically slow in ClosedXML and not how sheets are read in practice).
        long checksum = 0;
        foreach (var cell in ws.CellsUsed())
            checksum += cell.GetString().Length;
        return checksum;
    }
}
