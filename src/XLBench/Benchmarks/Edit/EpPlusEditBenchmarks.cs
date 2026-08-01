using BenchmarkDotNet.Attributes;
using OfficeOpenXml;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.Edit;

/// <summary>
/// EPPlus edit-and-recalculate: full scenario support.
///
/// <c>ExcelWorksheet.DeleteRow</c> shifts the rows below up and rewrites their formula ranges,
/// so the per-row <c>SUM</c> follows its row down without help. Deletes run bottom-up because
/// each one renumbers everything beneath it.
///
/// EPPlus ships a calculation engine, so <c>Workbook.Calculate()</c> evaluates the formulas here
/// rather than deferring to Excel.
/// </summary>
public class EpPlusEditBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        EpPlusLicense.Ensure();
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
        EpPlusLicense.Ensure();

        var total = double.NaN;
        var saved = EditOutput.Save("epplus",
            output => total = Edit(new MemoryStream(EditData.EditXlsx), output));
        return new EditResult(saved, total);
    }

    /// <inheritdoc cref="ClosedXmlEditBenchmarks"/>
    private static double Edit(Stream input, Stream? output)
    {
        using var pkg = new ExcelPackage(input);
        var ws = pkg.Workbook.Worksheets[0];

        var toDelete = EditData.RowsToDelete;
        for (var i = toDelete.Length - 1; i >= 0; i--)
            ws.DeleteRow(toDelete[i]);

        for (var row = EditData.FirstDataRow; row <= EditData.LastRowAfterEdit; row++)
            ws.Cells[row, 1].Value = EditData.ColumnAValue;

        pkg.Workbook.Calculate();

        var total = Convert.ToDouble(ws.Cells[EditData.LastRowAfterEdit, EditData.SumCol].Value);
        if (output is not null) pkg.SaveAs(output);
        return total;
    }
}
