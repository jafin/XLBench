using BenchmarkDotNet.Attributes;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using XLBench.Data;

namespace XLBench.Benchmarks.Edit;

/// <summary>
/// NPOI edit-and-recalculate: full scenario support, but row deletion has to be assembled from
/// two primitives.
///
/// NPOI has no "delete this row and close the gap" call. <c>ISheet.RemoveRow</c> only empties the
/// slot; closing it means a separate <c>ShiftRows</c> over everything below, which is also what
/// rewrites the shifted rows' <c>SUM</c> ranges. Two calls per deleted row, each touching every
/// row beneath it — so the cost here is quadratic in the number of deletions, and that is the
/// finding rather than an artifact of how the benchmark is written: no bulk API exists to use
/// instead.
///
/// The formula evaluator is real, so <c>EvaluateAll()</c> computes the new totals in-process.
/// </summary>
public class NpoiEditBenchmarks
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
        var saved = EditOutput.Save("npoi",
            output => total = Edit(new MemoryStream(EditData.EditXlsx), output));
        return new EditResult(saved, total);
    }

    /// <inheritdoc cref="ClosedXmlEditBenchmarks"/>
    private static double Edit(Stream input, Stream? output)
    {
        using var wb = new XSSFWorkbook(input);
        var sheet = wb.GetSheetAt(0);

        // NPOI row indices are 0-based; EditData's are 1-based sheet rows.
        var toDelete = EditData.RowsToDelete;
        for (var i = toDelete.Length - 1; i >= 0; i--)
        {
            var index = toDelete[i] - 1;
            if (sheet.GetRow(index) is { } row)
                sheet.RemoveRow(row);
            sheet.ShiftRows(index + 1, sheet.LastRowNum, -1);
        }

        for (var r = EditData.FirstDataRow; r <= EditData.LastRowAfterEdit; r++)
        {
            var row = sheet.GetRow(r - 1) ?? sheet.CreateRow(r - 1);
            var cell = row.GetCell(0) ?? row.CreateCell(0);
            cell.SetCellValue(EditData.ColumnAValue);
        }

        wb.GetCreationHelper().CreateFormulaEvaluator().EvaluateAll();

        var total = sheet.GetRow(EditData.LastRowAfterEdit - 1)
            .GetCell(EditData.SumCol - 1)
            .NumericCellValue;

        if (output is not null) wb.Write(output, leaveOpen: true);
        return total;
    }
}
