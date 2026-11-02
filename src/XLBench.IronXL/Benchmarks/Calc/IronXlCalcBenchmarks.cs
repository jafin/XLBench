using BenchmarkDotNet.Attributes;
using IronXL;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.Calc;

/// <summary>
/// IronXL change-prices-and-recalculate.
///
/// IronXL ships a calculation engine, and <c>WorkBook.EvaluateAll()</c> recomputes every formula
/// in process.
/// </summary>
public class IronXlCalcBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        IronXlLicense.Ensure();
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
        IronXlLicense.Ensure();

        var total = double.NaN;
        var saved = CalcOutput.Save("ironxl",
            output => total = Recalculate(new MemoryStream(CalcData.SourceXlsx), output));
        return new CalcResult(saved, total);
    }

    /// <inheritdoc cref="ClosedXmlCalcBenchmarks"/>
    private static double Recalculate(Stream input, Stream? output)
    {
        // WorkBook is not IDisposable — see IronXlReadBenchmarks.
        var wb = WorkBook.Load(input);
        var products = wb.GetWorkSheet(CalcData.ProductsSheet);

        // IronXL row and column numbers are 0-based; CalcData's are 1-based.
        for (var i = 0; i < CalcData.ProductCount; i++)
            products.SetCellValue(CalcData.FirstDataRow + i - 1, CalcData.UnitPriceCol - 1, CalcData.NewPrice(i));

        wb.EvaluateAll();

        var total = wb.GetWorkSheet(CalcData.SummarySheet)[CalcData.GrandTotalAddress].First().DoubleValue;

        if (output is not null)
        {
            // No SaveAs(Stream) overload — ToStream() materializes the workbook itself.
            using var ms = wb.ToStream();
            ms.CopyTo(output);
        }
        return total;
    }
}
