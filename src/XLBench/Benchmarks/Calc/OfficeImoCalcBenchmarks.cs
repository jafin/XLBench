using System.Globalization;
using BenchmarkDotNet.Attributes;
using OfficeIMO.Excel;
using XLBench.Data;

namespace XLBench.Benchmarks.Calc;

/// <summary>
/// OfficeIMO change-prices-and-recalculate: runs, but the result is incomplete.
///
/// <para><b>Excluded from the scenario.</b> The <c>[Benchmark]</c> and <c>[GlobalCleanup]</c>
/// attributes are commented out, so BenchmarkDotNet does not pick the class up, and
/// <see cref="CalcArtifacts"/> skips it. The reasons: wrong results (below), about 20 s per
/// operation against about one second for the other engines, and about 45 GB allocated per
/// operation. The code stays so the exclusion can be re-checked after an OfficeIMO bump —
/// restore the attributes and the <see cref="CalcArtifacts"/> entry.</para>
///
/// <c>Calculate()</c> runs OfficeIMO's own lightweight formula engine and writes the results back
/// as cached values. Each function the scenario uses works on its own — <c>VLOOKUP</c>,
/// <c>INDEX</c>/<c>MATCH</c>, <c>IFERROR</c>, <c>SUMIFS</c> and <c>COUNTIFS</c> all give Excel's
/// answer — but one combination does not.
///
/// <para>When <c>INDEX</c> is handed an error — here the <c>#N/A</c> of a <c>MATCH</c> that found
/// nothing — the engine leaves the cell uncalculated instead of returning the error. The
/// surrounding <c>IFERROR</c> then has nothing to catch, so the unknown-product orders get no
/// <c>Category</c> at all rather than <c>"Unknown"</c>. Every <c>SUMIFS</c> and <c>COUNTIFS</c>
/// reads that column, so the engine leaves them, and the grand total built on them, uncalculated
/// too: the benchmark reads <c>NaN</c> back. <c>INDEX</c> with an out-of-range position behaves the
/// same way. Every order's <c>Net</c> is still correct, since its lookups are plain
/// <c>VLOOKUP</c>s.</para>
///
/// <para>Read the timing with that in mind: the engine skips the summary's aggregates, the most
/// expensive formulas in the workbook, and is still an order of magnitude slower than the
/// other engines. With the unknown products
/// removed it computes every cell correctly and takes about half as long, so the failed
/// <c>INDEX</c> costs time as well as answers.</para>
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

    // Excluded from the run — see the class summary.
    // [Benchmark]
    public double ChangePricesAndRecalculate() => Recalculate(new MemoryStream(_bytes), output: null);

    // [GlobalCleanup]
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
            products.CellValue(CalcData.FirstDataRow + i, CalcData.UnitPriceCol, CalcData.NewPrice(i));

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
