using BenchmarkDotNet.Attributes;
using IronXL;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.MultiSheet;

/// <summary>
/// IronXL multi-sheet read: every sheet through <c>WorkBook.WorkSheets</c>, every populated cell
/// through <c>FilledCells</c>, as in <see cref="Read.IronXlReadBenchmarks"/>.
///
/// <para>Text cells are recognised by <c>IsText</c>, and <c>StringValue</c> is their plain
/// text.</para>
/// </summary>
public class IronXlSheetsBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        IronXlLicense.Ensure();
        SheetsData.EnsureLoaded();
        _bytes = SheetsData.SourceXlsx;
    }

    [Benchmark]
    public long OpenAndReadAllSheets() => Read(_bytes);

    /// <inheritdoc cref="ClosedXmlSheetsBenchmarks.Read"/>
    internal static long Read(byte[] bytes)
    {
        // WorkBook is not IDisposable — see IronXlReadBenchmarks.
        var wb = WorkBook.Load(new MemoryStream(bytes));

        long cells = 0;
        long textChars = 0;
        foreach (var ws in wb.WorkSheets)
        foreach (var cell in ws.FilledCells)
        {
            cells++;
            if (cell.IsText)
                textChars += (cell.StringValue ?? string.Empty).Length;
        }
        return SheetsData.Checksum(cells, textChars);
    }
}
