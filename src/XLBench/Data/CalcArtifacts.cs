using System.Diagnostics;
using System.Globalization;
using XLBench.Benchmarks.Calc;
using XLBench.Libraries;

namespace XLBench.Data;

/// <summary>
/// Runs every library's <c>ChangePricesAndRecalculate</c> once, unmeasured, writing the resulting
/// workbook to <c>output/</c> and then checking every recalculated formula in the saved file
/// against the values computed straight from the generated inputs (<c>dotnet run -- calc</c>).
///
/// <para>The benchmark reads back only the grand total, which is enough to stop the work being
/// optimized away but not enough to call the engine correct: two wrong answers can cancel out in a
/// sum. So the check re-opens the artifact and reads each order's looked-up <c>Category</c> and
/// <c>Net</c>, every <c>SUMIFS</c> and <c>COUNTIFS</c> cell in the summary grid, and the three
/// statistics, out of the package XML — see <see cref="SavedSheet"/> for why without a library.</para>
///
/// <para>The benchmarks save the same artifacts from <c>[GlobalCleanup]</c>; this exists so the
/// scenario can be verified in seconds after a change. The elapsed time it prints is a single cold
/// pass — indicative only, never a benchmark result.</para>
/// </summary>
public static class CalcArtifacts
{
    /// <summary>
    /// Relative tolerance for a recalculated number. The engines add the same values in different
    /// orders, so the last few ulps of a 1,000-term sum are expected to differ.
    /// </summary>
    private const double Tolerance = 1e-9;

    public static void WriteAll()
    {
        CalcData.EnsureLoaded();

        var expected = CalcData.Expected.GrandTotal;
        var totalCell = $"{CalcData.SummarySheet}!{CalcData.GrandTotalAddress}";

        Console.WriteLine(
            $"[XLBench] Calc scenario: {CalcData.OrderCount:N0} orders x 5 formulas looking up " +
            $"{CalcData.ProductCount} products, summarised by {CalcData.SummaryCategories.Length} categories x " +
            $"{CalcData.Regions.Length} regions; every price raised by {CalcData.PriceIncrease}.");
        Console.WriteLine(
            $"[XLBench] Expected {totalCell} after the price change: {Format(expected)}");
        Console.WriteLine($"[XLBench] Writing calc artifacts to {CalcOutput.Directory}");

        // Each entry is a library's [GlobalCleanup] save, invoked directly. One failing library
        // must not stop the others — a partial set is still reviewable, and the exception text
        // is the useful output.
        (string Library, string Slug, Func<CalcResult> Run)[] writers =
        [
            ("ClosedXML", "closedxml", ClosedXmlCalcBenchmarks.WriteArtifact),
            ("EPPlus", "epplus", EpPlusCalcBenchmarks.WriteArtifact),
            ("NPOI", "npoi", NpoiCalcBenchmarks.WriteArtifact),
            ("XLibur", "xlibur", XLiburCalcBenchmarks.WriteArtifact),
            // Excluded, like its [Benchmark] — see OfficeImoCalcBenchmarks. Uncomment to re-check it.
            // ("OfficeIMO", "officeimo", OfficeImoCalcBenchmarks.WriteArtifact),
            ("IronXL", "ironxl", IronXlCalcBenchmarks.WriteArtifact),
        ];

        // IronXL throws rather than watermarking when unlicensed (see IronXlLicense). Skipping it
        // with one line beats a stack trace in the middle of an otherwise clean run.
        if (!IronXlLicense.KeyAvailable)
        {
            writers = [.. writers.Where(w => w.Library != "IronXL")];
            Console.WriteLine("[XLBench] Skipping IronXL — XLBENCH_IRONXL_KEY not set (see README).");
        }

        var written = 0;
        var skipped = 0;
        var failed = 0;
        var wrong = 0;

        foreach (var (library, slug, run) in writers)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                var result = run();
                sw.Stop();

                if (!result.Saved)
                {
                    skipped++;
                    Console.WriteLine($"[XLBench] {library}: skipped (artifact locked).");
                    continue;
                }

                written++;

                var verification = Verify(CalcOutput.PathFor(slug));
                var totalOk = Matches(result.Total, expected);
                var ok = totalOk && verification.IsClean;
                if (!ok) wrong++;

                Console.WriteLine(
                    $"[XLBench] {library}: {(ok ? "OK" : "MISMATCH")} {totalCell}={Format(result.Total)}" +
                    $" — {verification.Describe()}" +
                    $" — {sw.ElapsedMilliseconds:N0} ms (single cold pass, not a benchmark result).");

                if (!totalOk)
                {
                    Console.WriteLine(
                        $"[XLBench]   {totalCell} read back from the library does not match: expected {Format(expected)}.");
                }
                foreach (var problem in verification.Problems)
                    Console.WriteLine($"[XLBench]   {problem}");
            }
#pragma warning disable S2221 // A broken library must not prevent the others from producing artifacts
            catch (Exception ex)
#pragma warning restore S2221
            {
                failed++;
                Console.WriteLine($"[XLBench] {library} failed: {ex}");
            }
        }

        Console.WriteLine($"[XLBench] Wrote {written} of {writers.Length} calc artifact(s)."
            + (skipped > 0 ? $" {skipped} skipped (file locked — close it in Excel and re-run)." : string.Empty)
            + (failed > 0 ? $" {failed} failed." : string.Empty)
            + (wrong > 0 ? $" {wrong} produced a wrong result." : string.Empty));
    }

    /// <summary>What the saved file says about every checked formula cell.</summary>
    private sealed class Verification
    {
        private const int MaxReported = 5;

        public int Cells { get; private set; }
        public int Bad { get; private set; }
        public List<string> Problems { get; } = [];

        /// <summary>Bad cells grouped by what they were meant to hold, e.g. "Orders!I (Net)".</summary>
        private readonly Dictionary<string, int> _badByKind = [];

        public bool IsClean => Bad == 0;

        public string Describe() =>
            IsClean
                ? $"{Cells:N0}/{Cells:N0} saved formula results verified"
                : $"{Cells - Bad:N0}/{Cells:N0} saved formula results verified, {Bad:N0} wrong or missing ("
                  + string.Join(", ", _badByKind.Select(kv => $"{kv.Key}: {kv.Value:N0}")) + ")";

        public void Ok() => Cells++;

        public void Fail(string kind, string problem)
        {
            Cells++;
            Bad++;
            _badByKind[kind] = _badByKind.GetValueOrDefault(kind) + 1;
            if (Problems.Count < MaxReported) Problems.Add(problem);
        }
    }

    /// <summary>
    /// Reads the formula columns out of the saved package and checks each cached result against
    /// <see cref="CalcData.Expected"/>. Only the first few problems are reported; the counts cover
    /// the rest.
    /// </summary>
    private static Verification Verify(string path)
    {
        var expected = CalcData.Expected;
        var v = new Verification();

        var net = SavedSheet.ReadColumn(path, CalcData.NetCol, CalcData.OrdersSheet);
        var category = SavedSheet.ReadColumn(path, CalcData.OrderCategoryCol, CalcData.OrdersSheet);
        for (var i = 0; i < CalcData.OrderCount; i++)
        {
            var row = CalcData.FirstDataRow + i;
            CheckNumber(v, "Orders Net", net, CalcData.OrdersSheet, row, CalcData.NetCol, expected.Net[i]);
            CheckText(v, "Orders Category", category, CalcData.OrdersSheet, row, CalcData.OrderCategoryCol,
                expected.Category[i]);
        }

        for (var col = CalcData.FirstRegionCol; col <= CalcData.LastRegionCol; col++)
        {
            var column = SavedSheet.ReadColumn(path, col, CalcData.SummarySheet);
            for (var c = 0; c < CalcData.SummaryCategories.Length; c++)
            {
                CheckNumber(v, "Summary SUMIFS", column, CalcData.SummarySheet, CalcData.FirstDataRow + c, col,
                    expected.Grid[c][col - CalcData.FirstRegionCol]);
            }
        }

        var counts = SavedSheet.ReadColumn(path, CalcData.CountCol, CalcData.SummarySheet);
        for (var c = 0; c < CalcData.SummaryCategories.Length; c++)
        {
            CheckNumber(v, "Summary COUNTIFS", counts, CalcData.SummarySheet, CalcData.FirstDataRow + c,
                CalcData.CountCol, expected.Counts[c]);
        }

        var stats = SavedSheet.ReadColumn(path, CalcData.StatValueCol, CalcData.SummarySheet);
        CheckNumber(v, "Summary SUM", stats, CalcData.SummarySheet, CalcData.GrandTotalRow, CalcData.StatValueCol,
            expected.GrandTotal);
        CheckNumber(v, "Summary AVERAGE", stats, CalcData.SummarySheet, CalcData.AverageNetRow, CalcData.StatValueCol,
            expected.AverageNet);
        CheckNumber(v, "Summary MAX", stats, CalcData.SummarySheet, CalcData.MaxNetRow, CalcData.StatValueCol,
            expected.MaxNet);

        return v;
    }

    private static void CheckNumber(Verification v, string kind, Dictionary<int, SavedSheet.Cell> column,
        string sheet, int row, int col, double expected)
    {
        var reference = $"{sheet}!{A1.ColumnLetter(col)}{row}";

        if (!column.TryGetValue(row, out var cell))
            v.Fail(kind, $"{reference} is absent from the saved sheet.");
        else if (cell.Number is not { } actual)
            v.Fail(kind, $"{reference} has no usable cached value (t=\"{cell.Type ?? "n"}\", v={cell.RawValue ?? "<none>"}," +
                         $" formula: {cell.Formula ?? "<none>"}).");
        else if (!Matches(actual, expected))
            v.Fail(kind, $"{reference} = {Format(actual)}, expected {Format(expected)} (formula: {cell.Formula ?? "<none>"}).");
        else
            v.Ok();
    }

    private static void CheckText(Verification v, string kind, Dictionary<int, SavedSheet.Cell> column,
        string sheet, int row, int col, string expected)
    {
        var reference = $"{sheet}!{A1.ColumnLetter(col)}{row}";

        if (!column.TryGetValue(row, out var cell))
            v.Fail(kind, $"{reference} is absent from the saved sheet.");
        else if (!string.Equals(cell.RawValue, expected, StringComparison.Ordinal))
            v.Fail(kind, $"{reference} = '{cell.RawValue ?? "<none>"}' (t=\"{cell.Type ?? "n"}\"), expected '{expected}'" +
                         $" (formula: {cell.Formula ?? "<none>"}).");
        else
            v.Ok();
    }

    private static bool Matches(double actual, double expected) =>
        double.IsFinite(actual)
        && Math.Abs(actual - expected) <= Tolerance * Math.Max(1d, Math.Abs(expected));

    private static string Format(double value) => value.ToString("R", CultureInfo.InvariantCulture);
}
