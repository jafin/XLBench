using BenchmarkDotNet.Attributes;
using ClosedXML.Excel;
using XLBench.Data;

namespace XLBench.Benchmarks.Edit;

/// <summary>
/// ClosedXML edit-and-recalculate: full scenario support through the high-level cell model.
///
/// <c>IXLRow.Delete()</c> shifts the rows below it up and rewrites their formula ranges, so each
/// row's <c>SUM</c> keeps pointing at its own row without any help. Rows are deleted bottom-up
/// because every delete renumbers what is beneath it.
///
/// ClosedXML owns a calculation engine, so <c>RecalculateAllFormulas()</c> is a real evaluation
/// rather than a flag for Excel to honour when the file is next opened.
/// </summary>
public class ClosedXmlEditBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        EditData.EnsureLoaded();
        _bytes = EditData.EditXlsx;
    }

    [Benchmark]
    public double EditAndRecalculate() => Edit(new MemoryStream(_bytes), output: null);

    [GlobalCleanup]
    public void SaveArtifact() => WriteArtifact();

    /// <summary>Runs one unmeasured pass, writing the artifact and reporting the total it produced.</summary>
    internal static EditResult WriteArtifact()
    {
        var total = double.NaN;
        var saved = EditOutput.Save("closedxml",
            output => total = Edit(new MemoryStream(EditData.EditXlsx), output));
        return new EditResult(saved, total);
    }

    /// <summary>
    /// Runs the scenario and returns the recalculated total of the last surviving row, so the
    /// result cannot be optimized away and callers can verify the recalculation actually happened.
    /// <paramref name="output"/> is null for the measured path — saving is not part of the scenario.
    /// </summary>
    private static double Edit(Stream input, Stream? output)
    {
        using var wb = new XLWorkbook(input);
        var ws = wb.Worksheets.First();

        var toDelete = EditData.RowsToDelete;
        for (var i = toDelete.Length - 1; i >= 0; i--)
            ws.Row(toDelete[i]).Delete();

        for (var row = EditData.FirstDataRow; row <= EditData.LastRowAfterEdit; row++)
            ws.Cell(row, 1).Value = EditData.ColumnAValue;

        wb.RecalculateAllFormulas();

        var total = ws.Cell(EditData.LastRowAfterEdit, EditData.SumCol).CachedValue.GetNumber();
        if (output is not null) wb.SaveAs(output);
        return total;
    }
}
