using BenchmarkDotNet.Attributes;
using OfficeOpenXml;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.Calc;

/// <summary>
/// EPPlus change-prices-and-recalculate: full scenario support.
///
/// <c>Workbook.Calculate()</c> builds a dependency chain over every formula and evaluates it with
/// EPPlus's own engine, which covers all of the scenario's functions.
/// </summary>
public class EpPlusCalcBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        EpPlusLicense.Ensure();
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
        EpPlusLicense.Ensure();

        var total = double.NaN;
        var saved = CalcOutput.Save("epplus",
            output => total = Recalculate(new MemoryStream(CalcData.SourceXlsx), output));
        return new CalcResult(saved, total);
    }

    /// <inheritdoc cref="ClosedXmlCalcBenchmarks"/>
    private static double Recalculate(Stream input, Stream? output)
    {
        using var pkg = new ExcelPackage(input);
        var products = pkg.Workbook.Worksheets[CalcData.ProductsSheet];

        for (var i = 0; i < CalcData.ProductCount; i++)
            products.Cells[CalcData.FirstDataRow + i, CalcData.UnitPriceCol].Value = CalcData.NewPrice(i);

        pkg.Workbook.Calculate();

        var total = Convert.ToDouble(pkg.Workbook.Worksheets[CalcData.SummarySheet]
            .Cells[CalcData.GrandTotalRow, CalcData.StatValueCol].Value);
        if (output is not null) pkg.SaveAs(output);
        return total;
    }
}
