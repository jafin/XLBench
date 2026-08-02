using BenchmarkDotNet.Attributes;
using XLBench.Data;
using XLibur.Excel;

namespace XLBench.Benchmarks.Edit;

/// <summary>
/// XLibur edit-and-recalculate: full scenario support.
///
/// The row and cell API is ClosedXML-shaped, so this mirrors
/// <see cref="ClosedXmlEditBenchmarks"/> line for line — the same <c>IXLRows.Delete()</c> takes
/// the whole deletion set, and <c>RecalculateAllFormulas()</c> drives a real calculation engine.
/// That makes the two rows in the results table a fairly direct comparison of two implementations
/// of the same design.
///
/// The implementations differ underneath: from 0.300.0 XLibur collapses the set into one
/// row-deletion map and re-points every formula against it in a single pass, where ClosedXML
/// deletes the rows one at a time and pays that pass per row. Handing both the set rather than
/// looping here is what lets the difference show.
/// </summary>
public class XLiburEditBenchmarks
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

    /// <inheritdoc cref="ClosedXmlEditBenchmarks.WriteArtifact"/>
    internal static EditResult WriteArtifact()
    {
        var total = double.NaN;
        var saved = EditOutput.Save("xlibur",
            output => total = Edit(new MemoryStream(EditData.EditXlsx), output));
        return new EditResult(saved, total);
    }

    /// <inheritdoc cref="ClosedXmlEditBenchmarks"/>
    private static double Edit(Stream input, Stream? output)
    {
        using var wb = new XLWorkbook(input);
        var ws = wb.Worksheets.First();

        ws.Rows(EditData.RowsToDeleteSpec).Delete();

        for (var row = EditData.FirstDataRow; row <= EditData.LastRowAfterEdit; row++)
            ws.Cell(row, 1).Value = EditData.ColumnAValue;

        wb.RecalculateAllFormulas();

        var total = ws.Cell(EditData.LastRowAfterEdit, EditData.SumCol).CachedValue.GetNumber();
        if (output is not null) wb.SaveAs(output);
        return total;
    }
}
