using BenchmarkDotNet.Attributes;
using Telerik.Documents.Spreadsheet.FormatProviders.OpenXml.Xlsx;
using Telerik.Documents.Spreadsheet.Model;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.Edit;

/// <summary>
/// RadSpreadProcessing edit-and-recalculate: full scenario support through the eager cell model.
///
/// <c>Rows[…]</c> takes a set of ranges as well as a single one, so the 166 scattered rows go in
/// one <c>RowSelection.Remove()</c> rather than a reverse-order loop. The shift and the formula
/// fixup below it are the library's problem, not the caller's.
///
/// RadSpreadProcessing owns a calculation engine and drives it from a dependency graph as cells
/// change, so there is no explicit recalculate step: by the time the writes return, column U
/// already holds the new totals. Reading one back is what proves it.
///
/// <para>The delete is where this scenario's whole cost sits — the two writes and the read after
/// it are noise beside it — and it is far and away the slowest of the six here even with undo
/// recording switched off (see <see cref="TelerikWorkbooks.WithoutHistory"/>). Deleting a row
/// through a model this rich is not a cheap operation.</para>
/// </summary>
public class TelerikEditBenchmarks
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
        var saved = EditOutput.Save("telerik",
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
        var provider = new XlsxFormatProvider();
        using var workbook = provider.Import(input, null).WithoutHistory();
        var worksheet = workbook.Worksheets[0];

        worksheet.Rows[EditData.RowsToDelete.Select(row => CellRange.FromRow(row - 1))].Remove();

        for (var row = EditData.FirstDataRow; row <= EditData.LastRowAfterEdit; row++)
            worksheet.Cells[row - 1, 0].SetValue(EditData.ColumnAValue);

        var total = TotalAt(worksheet, EditData.LastRowAfterEdit, EditData.SumCol);
        if (output is not null) provider.Export(workbook, output, null);
        return total;
    }

    /// <summary>
    /// Reads a formula cell's computed result as a number. <c>GetValue()</c> hands back the
    /// formula itself; its result is a second cell value behind
    /// <see cref="FormulaCellValue.GetResultValueAsCellValue"/>.
    /// </summary>
    internal static double TotalAt(Worksheet worksheet, int row, int column) =>
        worksheet.Cells[row - 1, column - 1].GetValue().Value switch
        {
            FormulaCellValue formula => formula.GetResultValueAsCellValue() is NumberCellValue result
                ? result.Value
                : double.NaN,
            NumberCellValue number => number.Value,
            _ => double.NaN,
        };
}
