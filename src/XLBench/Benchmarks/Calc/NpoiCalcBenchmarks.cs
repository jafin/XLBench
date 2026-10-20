using BenchmarkDotNet.Attributes;
using NPOI.XSSF.UserModel;
using XLBench.Data;

namespace XLBench.Benchmarks.Calc;

/// <summary>
/// NPOI change-prices-and-recalculate: full scenario support.
///
/// NPOI's formula evaluator is a port of Apache POI's, and <c>EvaluateAll()</c> evaluates every
/// formula cell in the workbook and stores the results as cached values. It evaluates
/// recursively, which is one reason the scenario keeps its dependency graph shallow.
/// </summary>
public class NpoiCalcBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        CalcData.EnsureLoaded();
        _bytes = CalcData.SourceXlsx;
    }

    [Benchmark]
    public double ChangePricesAndRecalculate() => Recalculate(new MemoryStream(_bytes), output: null);

    [GlobalCleanup]
    public void SaveArtifact() => WriteArtifact();

    /// <inheritdoc cref="ClosedXmlCalcBenchmarks.WriteArtifact"/>
    internal static CalcResult WriteArtifact()
    {
        var total = double.NaN;
        var saved = CalcOutput.Save("npoi",
            output => total = Recalculate(new MemoryStream(CalcData.SourceXlsx), output));
        return new CalcResult(saved, total);
    }

    /// <inheritdoc cref="ClosedXmlCalcBenchmarks"/>
    private static double Recalculate(Stream input, Stream? output)
    {
        using var wb = new XSSFWorkbook(input);

        // NPOI row and column indices are 0-based; CalcData's are 1-based.
        var products = wb.GetSheet(CalcData.ProductsSheet);
        for (var i = 0; i < CalcData.ProductCount; i++)
        {
            products.GetRow(CalcData.FirstDataRow + i - 1)
                .GetCell(CalcData.UnitPriceCol - 1)
                .SetCellValue(CalcData.NewPrice(i));
        }

        wb.GetCreationHelper().CreateFormulaEvaluator().EvaluateAll();

        var total = wb.GetSheet(CalcData.SummarySheet)
            .GetRow(CalcData.GrandTotalRow - 1)
            .GetCell(CalcData.StatValueCol - 1)
            .NumericCellValue;

        if (output is not null) wb.Write(output, leaveOpen: true);
        return total;
    }
}
