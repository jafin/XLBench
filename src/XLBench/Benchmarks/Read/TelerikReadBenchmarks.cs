using BenchmarkDotNet.Attributes;
using Telerik.Documents.Spreadsheet.FormatProviders.OpenXml.Xlsx;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.Read;

/// <summary>
/// RadSpreadProcessing read: an eager cell model behind a format-provider façade. Nothing is
/// read from or written to a file by the <c>Workbook</c> itself — an <see cref="XlsxFormatProvider"/>
/// imports a stream into the model and exports the model back out.
///
/// <para><b>Properties: Title only.</b> <c>Workbook.DocumentInfo</c> exposes Author, Description,
/// Keywords, Subject and Title — there is no Category. So this benchmark writes one of the two
/// properties the scenario asks for and the other libraries write both. It is a small difference
/// in a round trip dominated by import and export, but it is a difference; see the capability
/// matrix in the README before reading this row against theirs.</para>
/// </summary>
public class TelerikReadBenchmarks
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
    /// Round trip: import the workbook, set its Title, export it back out. Returns the serialized
    /// length so the export cannot be optimized away.
    /// </summary>
    [Benchmark]
    public long OpenAmendPropertiesAndSave()
    {
        var provider = new XlsxFormatProvider();
        using var workbook = provider.Import(new MemoryStream(_propertiesBytes), null).WithoutHistory();

        workbook.DocumentInfo.Title = PropertiesData.Title;

        using var output = new MemoryStream();
        provider.Export(workbook, output, null);
        return output.Length;
    }

    [Benchmark]
    public long OpenAndReadAll()
    {
        using var workbook = new XlsxFormatProvider().Import(new MemoryStream(_bytes), null).WithoutHistory();
        var worksheet = workbook.Worksheets[0];

        // RadSpreadProcessing has no "enumerate the populated cells" call — no CellsUsed(), no
        // row iterator. Its own documentation walks the used range by index, so that is what the
        // idiomatic read looks like here, and indexing is not the pathological path it is in the
        // libraries this suite avoids it for.
        var used = worksheet.UsedCellRange;
        long checksum = 0;
        for (var row = used.FromIndex.RowIndex; row <= used.ToIndex.RowIndex; row++)
        for (var col = used.FromIndex.ColumnIndex; col <= used.ToIndex.ColumnIndex; col++)
            checksum += worksheet.Cells[row, col].GetValue().Value.RawValue.Length;
        return checksum;
    }
}
