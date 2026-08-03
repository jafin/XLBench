using BenchmarkDotNet.Attributes;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using XLBench.Data;

namespace XLBench.Benchmarks.Read;

public class NpoiReadBenchmarks
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

    /// <inheritdoc cref="ClosedXmlReadBenchmarks.OpenAmendPropertiesAndSave"/>
    /// <remarks>
    /// NPOI reaches the document properties through the OOXML package rather than the workbook:
    /// <c>GetProperties().CoreProperties</c> is the <c>docProps/core.xml</c> part itself, which is
    /// why Title and Category are set on it directly rather than on <c>XSSFWorkbook</c>.
    /// </remarks>
    [Benchmark]
    public long OpenAmendPropertiesAndSave()
    {
        using var wb = new XSSFWorkbook(new MemoryStream(_propertiesBytes));

        var core = wb.GetProperties().CoreProperties;
        core.Title = PropertiesData.Title;
        core.Category = PropertiesData.Category;

        using var output = new MemoryStream();
        wb.Write(output, leaveOpen: true);
        return output.Length;
    }

    [Benchmark]
    public long OpenAndReadAll()
    {
        using var wb = new XSSFWorkbook(new MemoryStream(_bytes));
        var sheet = wb.GetSheetAt(0);

        // Iterate rows and their populated cells rather than random GetRow/GetCell access.
        long checksum = 0;
        foreach (IRow row in sheet)
        {
            foreach (var cell in row.Cells)
                checksum += (cell.ToString() ?? string.Empty).Length;
        }
        return checksum;
    }
}
