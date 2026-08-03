using BenchmarkDotNet.Attributes;
using IronXL;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.Insert;

/// <summary>
/// IronXL insert-columns-and-recalculate: full scenario support.
///
/// <c>Range.InsertColumns(index, count)</c> takes the whole insert in one call, and IronXL ships a
/// calculation engine, so <c>WorkBook.EvaluateAll()</c> recomputes the widened row totals in
/// process.
/// </summary>
public class IronXlInsertBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        IronXlLicense.Ensure();
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
        IronXlLicense.Ensure();

        var total = double.NaN;
        var saved = InsertOutput.Save("ironxl",
            output => total = Insert(new MemoryStream(InsertData.SourceXlsx), output));
        return new InsertResult(saved, total);
    }

    /// <inheritdoc cref="ClosedXmlInsertBenchmarks"/>
    private static double Insert(Stream input, Stream? output)
    {
        // WorkBook is not IDisposable — see IronXlReadBenchmarks.
        var wb = WorkBook.Load(input);
        var ws = wb.WorkSheets[0];

        // IronXL row and column numbers are 0-based; InsertData's are 1-based.
        ws.InsertColumns(InsertData.InsertAtColumn - 1, InsertData.InsertColumnCount);

        for (var row = InsertData.FirstDataRow; row <= InsertData.LastDataRow; row++)
        for (var col = InsertData.InsertAtColumn; col < InsertData.AfterInsertedColumn; col++)
            ws.SetCellValue(row - 1, col - 1, InsertData.InsertedValue);

        wb.EvaluateAll();

        var total = ws[$"{InsertData.SumColLetter}{InsertData.LastDataRow}"].First().DoubleValue;

        if (output is not null)
        {
            // No SaveAs(Stream) overload — ToStream() materializes the workbook itself.
            using var ms = wb.ToStream();
            ms.CopyTo(output);
        }
        return total;
    }
}
