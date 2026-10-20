using BenchmarkDotNet.Attributes;
using ClosedXML.Excel;
using XLBench.Data;

namespace XLBench.Benchmarks.Calc;

/// <summary>
/// ClosedXML change-prices-and-recalculate: full scenario support.
///
/// The price writes go through the ordinary cell model, and <c>RecalculateAllFormulas()</c> runs
/// ClosedXML's own calculation engine over every formula in the workbook. All of the scenario's
/// functions — <c>VLOOKUP</c>, <c>INDEX</c>/<c>MATCH</c>, <c>IFERROR</c>, <c>SUMIFS</c>,
/// <c>COUNTIFS</c> — are evaluated in process.
///
/// <para>As in the insert scenario, a plain <c>SaveAs</c> writes formulas without their cached
/// results, so the artifact save opts in with <c>evaluateFormulae: true</c>. That is outside the
/// measured region and only means the saved workbook carries the results that
/// <see cref="CalcArtifacts"/> checks.</para>
/// </summary>
public class ClosedXmlCalcBenchmarks
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

    /// <summary>Runs one unmeasured pass, writing the artifact and reporting the total it produced.</summary>
    internal static CalcResult WriteArtifact()
    {
        var total = double.NaN;
        var saved = CalcOutput.Save("closedxml",
            output => total = Recalculate(new MemoryStream(CalcData.SourceXlsx), output));
        return new CalcResult(saved, total);
    }

    /// <summary>
    /// Runs the scenario and returns the recalculated grand total, so the result cannot be
    /// optimized away and callers have something to check. Every other formula is verified from
    /// the saved artifact by <see cref="CalcArtifacts"/>; <paramref name="output"/> is null for the
    /// measured path, since saving is not part of the scenario.
    /// </summary>
    private static double Recalculate(Stream input, Stream? output)
    {
        using var wb = new XLWorkbook(input);
        var products = wb.Worksheet(CalcData.ProductsSheet);

        for (var i = 0; i < CalcData.ProductCount; i++)
            products.Cell(CalcData.FirstDataRow + i, CalcData.UnitPriceCol).Value = CalcData.NewPrice(i);

        wb.RecalculateAllFormulas();

        var total = wb.Worksheet(CalcData.SummarySheet)
            .Cell(CalcData.GrandTotalRow, CalcData.StatValueCol)
            .CachedValue.GetNumber();
        if (output is not null) wb.SaveAs(output, validate: false, evaluateFormulae: true);
        return total;
    }
}
