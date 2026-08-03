using BenchmarkDotNet.Attributes;
using OfficeOpenXml;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.Insert;

/// <summary>
/// EPPlus insert-columns-and-recalculate: full scenario support.
///
/// <c>ExcelWorksheet.InsertColumn(from, count)</c> takes the whole insert in one call — it shifts
/// the columns right and rewrites the references that span them, so each row's <c>SUM(A:T)</c>
/// widens to <c>SUM(A:V)</c> on its own.
///
/// EPPlus ships a calculation engine, so <c>Workbook.Calculate()</c> evaluates the widened
/// formulas here rather than deferring to Excel.
/// </summary>
public class EpPlusInsertBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        EpPlusLicense.Ensure();
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
        EpPlusLicense.Ensure();

        var total = double.NaN;
        var saved = InsertOutput.Save("epplus",
            output => total = Insert(new MemoryStream(InsertData.SourceXlsx), output));
        return new InsertResult(saved, total);
    }

    /// <inheritdoc cref="ClosedXmlInsertBenchmarks"/>
    private static double Insert(Stream input, Stream? output)
    {
        using var pkg = new ExcelPackage(input);
        var ws = pkg.Workbook.Worksheets[0];

        ws.InsertColumn(InsertData.InsertAtColumn, InsertData.InsertColumnCount);

        for (var row = InsertData.FirstDataRow; row <= InsertData.LastDataRow; row++)
        for (var col = InsertData.InsertAtColumn; col < InsertData.AfterInsertedColumn; col++)
            ws.Cells[row, col].Value = InsertData.InsertedValue;

        pkg.Workbook.Calculate();

        var total = Convert.ToDouble(ws.Cells[InsertData.LastDataRow, InsertData.SumCol].Value);
        if (output is not null) pkg.SaveAs(output);
        return total;
    }
}
