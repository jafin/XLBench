using BenchmarkDotNet.Attributes;
using IronXL;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.Edit;

/// <summary>
/// IronXL edit-and-recalculate: full scenario support.
///
/// <c>RangeRow.RemoveRow()</c> deletes the row and shifts everything below it up, and IronXL
/// ships a calculation engine, so <c>WorkBook.EvaluateAll()</c> recomputes the row totals in
/// process. Rows are deleted bottom-up, as everywhere else, because each delete renumbers what
/// follows.
///
/// <para><c>WorkSheet.Rows</c> is re-materialized after every delete rather than cached: it is a
/// snapshot of the sheet's rows, so a cached array would hold references to rows whose numbers
/// the previous delete already changed.</para>
/// </summary>
public class IronXlEditBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        IronXlLicense.Ensure();
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
        IronXlLicense.Ensure();

        var total = double.NaN;
        var saved = EditOutput.Save("ironxl",
            output => total = Edit(new MemoryStream(EditData.EditXlsx), output));
        return new EditResult(saved, total);
    }

    /// <inheritdoc cref="ClosedXmlEditBenchmarks"/>
    private static double Edit(Stream input, Stream? output)
    {
        // WorkBook is not IDisposable — see IronXlReadBenchmarks.
        var wb = WorkBook.Load(input);
        var ws = wb.WorkSheets[0];

        // IronXL row numbers are 0-based; EditData's are 1-based sheet rows.
        var toDelete = EditData.RowsToDelete;
        for (var i = toDelete.Length - 1; i >= 0; i--)
            ws.RemoveRow(toDelete[i] - 1);

        for (var row = EditData.FirstDataRow; row <= EditData.LastRowAfterEdit; row++)
            ws.SetCellValue(row - 1, 0, EditData.ColumnAValue);

        wb.EvaluateAll();

        var total = ws[$"{EditData.SumColLetter}{EditData.LastRowAfterEdit}"].First().DoubleValue;

        if (output is not null)
        {
            // No SaveAs(Stream) overload — ToStream() materializes the workbook itself.
            using var ms = wb.ToStream();
            ms.CopyTo(output);
        }
        return total;
    }
}
