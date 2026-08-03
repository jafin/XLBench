using BenchmarkDotNet.Attributes;
using ClosedXML.Excel;
using XLBench.Data;

namespace XLBench.Benchmarks.Insert;

/// <summary>
/// ClosedXML insert-columns-and-recalculate: full scenario support through the high-level cell
/// model.
///
/// <c>IXLColumn.InsertColumnsBefore(n)</c> pushes the columns from B rightwards and rewrites every
/// reference that crosses them, so each row's <c>SUM(A:T)</c> comes back as <c>SUM(A:V)</c> and
/// takes the new columns in without any help. One structural call, one workbook-wide fixup.
///
/// ClosedXML owns a calculation engine, so <c>RecalculateAllFormulas()</c> is a real evaluation
/// rather than a flag for Excel to honour when the file is next opened.
///
/// <para>It is, however, the one library here that does not persist what it computed: a plain
/// <c>SaveAs</c> writes each formula cell as <c>&lt;f&gt;</c> with no cached <c>&lt;v&gt;</c>, so
/// the totals exist in memory and nowhere in the file. The artifact save therefore opts in with
/// <c>evaluateFormulae: true</c>. That is outside the measured region and changes no timing — it
/// only means the saved workbook carries its results like every other library's does, which is
/// what <see cref="InsertArtifacts"/> checks.</para>
/// </summary>
public class ClosedXmlInsertBenchmarks
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

    /// <summary>Runs one unmeasured pass, writing the artifact and reporting the total it produced.</summary>
    internal static InsertResult WriteArtifact()
    {
        var total = double.NaN;
        var saved = InsertOutput.Save("closedxml",
            output => total = Insert(new MemoryStream(InsertData.SourceXlsx), output));
        return new InsertResult(saved, total);
    }

    /// <summary>
    /// Runs the scenario and returns the recalculated total of the last data row, so the result
    /// cannot be optimized away and callers have something to check. Every other row is verified
    /// from the saved artifact by <see cref="InsertArtifacts"/>; <paramref name="output"/> is null
    /// for the measured path, since saving is not part of the scenario.
    /// </summary>
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
        if (output is not null) wb.SaveAs(output, validate: false, evaluateFormulae: true);
        return total;
    }
}
