using BenchmarkDotNet.Attributes;
using IronXL;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.Read;

public class IronXlReadBenchmarks
{
    private byte[] _bytes = null!;
    private byte[] _propertiesBytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        IronXlLicense.Ensure();
        _bytes = TestData.ReadXlsx;
        PropertiesData.EnsureLoaded();
        _propertiesBytes = PropertiesData.SourceXlsx;
    }

    /// <inheritdoc cref="ClosedXmlReadBenchmarks.OpenAmendPropertiesAndSave"/>
    /// <remarks>
    /// IronXL exposes the document properties as <c>WorkBook.Metadata</c>. There is also no
    /// <c>SaveAs(Stream)</c> overload, so <c>ToStream()</c> materializes the workbook and that
    /// stream's length is what the round trip produced.
    ///
    /// <para>WorkBook is not IDisposable — unlike every other eager-model library here, there is
    /// no deterministic release of the loaded workbook; it is left to the GC.</para>
    /// </remarks>
    [Benchmark]
    public long OpenAmendPropertiesAndSave()
    {
        var wb = WorkBook.Load(new MemoryStream(_propertiesBytes));

        wb.Metadata.Title = PropertiesData.Title;
        wb.Metadata.Category = PropertiesData.Category;

        using var output = wb.ToStream();
        return output.Length;
    }

    [Benchmark]
    public long OpenAndReadAll()
    {
        var wb = WorkBook.Load(new MemoryStream(_bytes));
        var ws = wb.DefaultWorkSheet;

        // FilledCells is IronXL's populated-cell enumeration — the equivalent of ClosedXML's
        // CellsUsed(), and the idiomatic alternative to random GetCellAt(row, col) access.
        long checksum = 0;
        foreach (var cell in ws.FilledCells)
            checksum += (cell.StringValue ?? string.Empty).Length;
        return checksum;
    }
}
