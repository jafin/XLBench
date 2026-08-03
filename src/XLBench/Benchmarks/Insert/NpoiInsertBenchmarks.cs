using BenchmarkDotNet.Attributes;
using NPOI.XSSF.UserModel;
using XLBench.Data;

namespace XLBench.Benchmarks.Insert;

/// <summary>
/// NPOI insert-columns-and-recalculate: full scenario support, but spelled as a shift rather than
/// an insert.
///
/// There is no "insert n columns here"; the equivalent is <c>XSSFSheet.ShiftColumns</c>, moving
/// the block that has to make way. It is a real structural call and not a workaround —
/// internally it builds a <c>FormulaShifter</c> for the column move and re-points formulas, named
/// ranges, conditional formatting and hyperlinks against it — but it lives on the concrete
/// <c>XSSFSheet</c>, not on <c>ISheet</c>, so the sheet is cast. Its row counterpart
/// <c>ShiftRows</c> is on the interface; the asymmetry is NPOI's.
///
/// <para>Note the contrast with the same library's row delete, which has no bulk call at all and
/// pays a whole-sheet shift per row. Here one call covers the whole edit.</para>
///
/// The formula evaluator is real, so <c>EvaluateAll()</c> computes the widened totals in-process.
/// </summary>
public class NpoiInsertBenchmarks
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
        var saved = InsertOutput.Save("npoi",
            output => total = Insert(new MemoryStream(InsertData.SourceXlsx), output));
        return new InsertResult(saved, total);
    }

    /// <inheritdoc cref="ClosedXmlInsertBenchmarks"/>
    private static double Insert(Stream input, Stream? output)
    {
        using var wb = new XSSFWorkbook(input);

        // ShiftColumns is on XSSFSheet rather than ISheet. NPOI column and row indices are
        // 0-based; InsertData's are 1-based.
        var sheet = (XSSFSheet)wb.GetSheetAt(0);
        sheet.ShiftColumns(
            InsertData.InsertAtColumn - 1,
            InsertData.LastColumnBeforeInsert - 1,
            InsertData.InsertColumnCount);

        for (var r = InsertData.FirstDataRow; r <= InsertData.LastDataRow; r++)
        {
            var row = sheet.GetRow(r - 1) ?? sheet.CreateRow(r - 1);
            for (var col = InsertData.InsertAtColumn; col < InsertData.AfterInsertedColumn; col++)
            {
                var cell = row.GetCell(col - 1) ?? row.CreateCell(col - 1);
                cell.SetCellValue(InsertData.InsertedValue);
            }
        }

        wb.GetCreationHelper().CreateFormulaEvaluator().EvaluateAll();

        var total = sheet.GetRow(InsertData.LastDataRow - 1)
            .GetCell(InsertData.SumCol - 1)
            .NumericCellValue;

        if (output is not null) wb.Write(output, leaveOpen: true);
        return total;
    }
}
