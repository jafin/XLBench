using System.Globalization;
using BenchmarkDotNet.Attributes;
using OfficeIMO.Excel;
using XLBench.Data;

namespace XLBench.Benchmarks.Insert;

/// <summary>
/// OfficeIMO insert-columns-and-recalculate: full scenario support.
///
/// <c>ExcelSheet.InsertColumns(first, count)</c> is one structural call that pushes the columns
/// right and widens every reference crossing them, so each row's <c>SUM(A:T)</c> comes back as
/// <c>SUM(A:V)</c>. The call is transactional — it plans the mutation and returns post-edit
/// diagnostics — and that bookkeeping is part of what is measured here.
///
/// <c>Calculate()</c> runs OfficeIMO's own lightweight formula engine and writes the totals back
/// as cached values, so the saved file carries its results.
/// </summary>
public class OfficeImoInsertBenchmarks
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

    /// <inheritdoc cref="ClosedXmlInsertBenchmarks.WriteArtifact"/>
    internal static InsertResult WriteArtifact()
    {
        var total = double.NaN;
        var saved = InsertOutput.Save("officeimo",
            output => total = Insert(new MemoryStream(InsertData.SourceXlsx), output));
        return new InsertResult(saved, total);
    }

    /// <inheritdoc cref="ClosedXmlInsertBenchmarks"/>
    private static double Insert(Stream input, Stream? output)
    {
        using var doc = ExcelDocument.Load(input);
        var ws = doc.Sheets[0];

        ws.InsertColumns(InsertData.InsertAtColumn, InsertData.InsertColumnCount);

        for (var row = InsertData.FirstDataRow; row <= InsertData.LastDataRow; row++)
        for (var col = InsertData.InsertAtColumn; col < InsertData.AfterInsertedColumn; col++)
            ws.CellValue(row, col, InsertData.InsertedValue);

        doc.Calculate();

        ws.TryGetCachedFormulaValue(InsertData.LastDataRow, InsertData.SumCol, out var cached);
        var total = double.Parse(cached!, NumberStyles.Float, CultureInfo.InvariantCulture);
        if (output is not null) doc.Save(output);
        return total;
    }
}
