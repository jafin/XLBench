using System.Globalization;
using BenchmarkDotNet.Attributes;
using OfficeIMO.Excel;
using XLBench.Data;

namespace XLBench.Benchmarks.Edit;

/// <summary>
/// OfficeIMO edit-and-recalculate: full scenario support.
///
/// <c>ExcelSheet.DeleteRows(first, count)</c> deletes a contiguous block, shifts the rows below it
/// up and rewrites the formula references that cross it. The rows this scenario deletes are not
/// contiguous, so the set is walked bottom-up with one call per row, as with NPOI.
///
/// <c>Calculate()</c> runs OfficeIMO's own lightweight formula engine, which covers <c>SUM</c>, and
/// writes the results back as cached values. Formulas it does not support would be left for Excel
/// to compute on open; none appear in this workbook.
/// </summary>
public class OfficeImoEditBenchmarks
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
        var saved = EditOutput.Save("officeimo",
            output => total = Edit(new MemoryStream(EditData.EditXlsx), output));
        return new EditResult(saved, total);
    }

    /// <inheritdoc cref="ClosedXmlEditBenchmarks"/>
    private static double Edit(Stream input, Stream? output)
    {
        using var doc = ExcelDocument.Load(input);
        var ws = doc.Sheets[0];

        for (var i = EditData.RowsToDelete.Length - 1; i >= 0; i--)
            ws.DeleteRows(EditData.RowsToDelete[i]);

        for (var row = EditData.FirstDataRow; row <= EditData.LastRowAfterEdit; row++)
            ws.CellValue(row, 1, EditData.ColumnAValue);

        doc.Calculate();

        ws.TryGetCachedFormulaValue(EditData.LastRowAfterEdit, EditData.SumCol, out var cached);
        var total = double.Parse(cached!, NumberStyles.Float, CultureInfo.InvariantCulture);
        if (output is not null) doc.Save(output);
        return total;
    }
}
