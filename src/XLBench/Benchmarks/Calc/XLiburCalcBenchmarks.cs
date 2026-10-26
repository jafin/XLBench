using BenchmarkDotNet.Attributes;
using XLBench.Data;
using XLibur.Excel;

namespace XLBench.Benchmarks.Calc;

/// <summary>
/// XLibur change-prices-and-recalculate: full scenario support.
///
/// The API is ClosedXML-shaped, so this mirrors <see cref="ClosedXmlCalcBenchmarks"/> line for
/// line, and <c>RecalculateAllFormulas()</c> drives XLibur's own calculation engine. With the calls
/// identical, the difference between the two rows in the results table is the engine.
/// </summary>
public class XLiburCalcBenchmarks
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
        var saved = CalcOutput.Save("xlibur",
            output => total = Recalculate(new MemoryStream(CalcData.SourceXlsx), output));
        return new CalcResult(saved, total);
    }

    /// <inheritdoc cref="ClosedXmlCalcBenchmarks"/>
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
        if (output is not null) wb.SaveAs(output);
        return total;
    }
}
