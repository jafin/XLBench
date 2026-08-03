using BenchmarkDotNet.Attributes;
using XLBench.Data;
using XLibur.Excel;

namespace XLBench.Benchmarks.Insert;

/// <summary>
/// XLibur insert-columns-and-recalculate: full scenario support.
///
/// The column API is ClosedXML-shaped, so this mirrors <see cref="ClosedXmlInsertBenchmarks"/>
/// line for line — the same <c>IXLColumn.InsertColumnsBefore(n)</c>, and
/// <c>RecalculateAllFormulas()</c> drives a real calculation engine. As in the edit scenario,
/// what differs between the two rows in the results table is the engine, not the call.
/// </summary>
public class XLiburInsertBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        InsertData.EnsureLoaded();
        _bytes = InsertData.SourceXlsx;
    }

    [Benchmark]
    public double InsertColumnsAndRecalculate() => Insert(new MemoryStream(_bytes), output: null);

    [GlobalCleanup]
    public void SaveArtifact() => WriteArtifact();

    /// <inheritdoc cref="ClosedXmlInsertBenchmarks.WriteArtifact"/>
    internal static InsertResult WriteArtifact()
    {
        var total = double.NaN;
        var saved = InsertOutput.Save("xlibur",
            output => total = Insert(new MemoryStream(InsertData.SourceXlsx), output));
        return new InsertResult(saved, total);
    }

    /// <inheritdoc cref="ClosedXmlInsertBenchmarks"/>
    private static double Insert(Stream input, Stream? output)
    {
        using var wb = new XLWorkbook(input);
        var ws = wb.Worksheets.First();

        ws.Column(InsertData.InsertAtColumn).InsertColumnsBefore(InsertData.InsertColumnCount);

        for (var row = InsertData.FirstDataRow; row <= InsertData.LastDataRow; row++)
        for (var col = InsertData.InsertAtColumn; col < InsertData.AfterInsertedColumn; col++)
            ws.Cell(row, col).Value = InsertData.InsertedValue;

        wb.RecalculateAllFormulas();

        var total = ws.Cell(InsertData.LastDataRow, InsertData.SumCol).CachedValue.GetNumber();
        if (output is not null) wb.SaveAs(output);
        return total;
    }
}
