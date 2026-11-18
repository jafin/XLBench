using System.Globalization;
using BenchmarkDotNet.Attributes;
using OfficeIMO.Excel;
using XLBench.Data;

namespace XLBench.Benchmarks.Calc;

/// <summary>
/// OfficeIMO change-prices-and-recalculate: full scenario support.
///
/// <c>Calculate()</c> runs OfficeIMO's own formula engine and writes the results back as cached
/// values. Up to 3.5.3 an error passed to <c>INDEX</c> — here the <c>#N/A</c> of a <c>MATCH</c>
/// that found nothing — left the cell uncalculated, so the surrounding <c>IFERROR</c> had nothing
/// to catch and the summary's <c>SUMIFS</c>, <c>COUNTIFS</c> and grand total stayed empty. Since
/// 3.6.0 the error reaches <c>IFERROR</c>, and all 2,048 results match.
/// </summary>
public class OfficeImoCalcBenchmarks
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
        var saved = CalcOutput.Save("officeimo",
            output => total = Recalculate(new MemoryStream(CalcData.SourceXlsx), output));
        return new CalcResult(saved, total);
    }

    /// <inheritdoc cref="ClosedXmlCalcBenchmarks"/>
    private static double Recalculate(Stream input, Stream? output)
    {
        using var doc = ExcelDocument.Load(input);
        var products = doc[CalcData.ProductsSheet];

        for (var i = 0; i < CalcData.ProductCount; i++)
            products.Cell(CalcData.FirstDataRow + i, CalcData.UnitPriceCol).SetValue(CalcData.NewPrice(i));

        doc.Calculate();

        // An uncalculated cell has no cached value, which is the documented outcome here — read
        // it as NaN rather than throw, so the benchmark still measures the run.
        doc[CalcData.SummarySheet].TryGetCachedFormulaValue(
            CalcData.GrandTotalRow, CalcData.StatValueCol, out var cached);
        var total = double.TryParse(cached, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)
            ? d
            : double.NaN;
        if (output is not null) doc.Save(output);
        return total;
    }
}
