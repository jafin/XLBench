using BenchmarkDotNet.Attributes;
using Telerik.Documents.Spreadsheet.FormatProviders.OpenXml.Xlsx;
using XLBench.Benchmarks.Edit;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.Insert;

/// <summary>
/// RadSpreadProcessing insert-columns-and-recalculate: full scenario support through the eager
/// cell model.
///
/// <c>ColumnSelection.Insert()</c> pushes the selected columns rightwards and rewrites every
/// reference that crosses them, so each row's <c>SUM(A:T)</c> comes back as <c>SUM(A:V)</c> and
/// takes the new columns in without any help. One structural call, one workbook-wide fixup.
///
/// As in <see cref="TelerikEditBenchmarks"/> there is no explicit recalculate step — the
/// calculation engine follows the dependency graph as the cells change.
/// </summary>
public class TelerikInsertBenchmarks
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
        var saved = InsertOutput.Save("telerik",
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
        var provider = new XlsxFormatProvider();
        using var workbook = provider.Import(input, null).WithoutHistory();
        var worksheet = workbook.Worksheets[0];

        worksheet.Columns[InsertData.InsertAtColumn - 1, InsertData.AfterInsertedColumn - 2].Insert();

        for (var row = InsertData.FirstDataRow; row <= InsertData.LastDataRow; row++)
        for (var col = InsertData.InsertAtColumn; col < InsertData.AfterInsertedColumn; col++)
            worksheet.Cells[row - 1, col - 1].SetValue(InsertData.InsertedValue);

        var total = TelerikEditBenchmarks.TotalAt(worksheet, InsertData.LastDataRow, InsertData.SumCol);
        if (output is not null) provider.Export(workbook, output, null);
        return total;
    }
}
