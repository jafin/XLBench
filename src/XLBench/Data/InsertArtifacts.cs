using System.Diagnostics;
using System.Globalization;
using XLBench.Benchmarks.Insert;
using XLBench.Libraries;

namespace XLBench.Data;

/// <summary>
/// Runs every library's <c>InsertColumnsAndRecalculate</c> once, unmeasured, writing the resulting
/// workbook to <c>output/</c> and then checking <i>every</i> row total in the saved file against
/// the value computed straight from the CSV (<c>dotnet run -- insert</c>).
///
/// <para>The benchmark itself only reads one cell back, which is enough to stop the work being
/// optimized away but not enough to call the result correct. What the scenario claims is that the
/// insert widened all 500 row totals and that they survived serialization, so the check re-opens
/// the artifact and reads all 500 out of the package XML — see <see cref="SavedSheet"/> for why
/// that is done without a library.</para>
///
/// <para>The benchmarks save the same artifacts from <c>[GlobalCleanup]</c>, so a normal run
/// already refreshes them; this exists so the scenario can be verified and eyeballed in seconds
/// after a change, rather than sitting through a measured run. The elapsed time it prints is a
/// single cold pass — indicative only, never a benchmark result.</para>
/// </summary>
public static class InsertArtifacts
{
    /// <summary>
    /// Relative tolerance for a recalculated total. The libraries sum the same twenty-two doubles
    /// but not necessarily in the same order, so the last few ulps are expected to differ.
    /// </summary>
    private const double Tolerance = 1e-9;

    public static void WriteAll()
    {
        InsertData.EnsureLoaded();

        var expected = InsertData.ExpectedLastTotal;
        var lastCell = $"{InsertData.SumColLetter}{InsertData.LastDataRow}";

        Console.WriteLine(
            $"[XLBench] Insert scenario: {InsertData.RowCount} rows x {EditData.ColCount} columns; " +
            $"inserting {InsertData.InsertColumnCount} columns of {InsertData.InsertedValue} before " +
            $"column {A1.ColumnLetter(InsertData.InsertAtColumn)} must widen every " +
            $"SUM(A:{A1.ColumnLetter(EditData.ColCount)}) to SUM(A:{InsertData.LastDataColLetter}).");
        Console.WriteLine(
            $"[XLBench] Expected {lastCell} after the insert: {expected.ToString("R", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"[XLBench] Writing insert artifacts to {InsertOutput.Directory}");

        // Each entry is a library's [GlobalCleanup] save, invoked directly. One failing library
        // must not stop the others — a partial set is still reviewable, and the exception text
        // is the useful output.
        (string Library, string Slug, Func<InsertResult> Run)[] writers =
        [
            ("ClosedXML", "closedxml", ClosedXmlInsertBenchmarks.WriteArtifact),
            ("EPPlus", "epplus", EpPlusInsertBenchmarks.WriteArtifact),
            ("NPOI", "npoi", NpoiInsertBenchmarks.WriteArtifact),
            ("OpenXML SDK", "openxml", OpenXmlInsertBenchmarks.WriteArtifact),
            ("XLibur", "xlibur", XLiburInsertBenchmarks.WriteArtifact),
            ("Telerik", "telerik", TelerikInsertBenchmarks.WriteArtifact),
            ("IronXL", "ironxl", IronXlInsertBenchmarks.WriteArtifact),
        ];

        // IronXL throws rather than watermarking when unlicensed (see IronXlLicense). Skipping it
        // with one line beats a stack trace in the middle of an otherwise clean run.
        if (!IronXlLicense.KeyAvailable)
        {
            writers = [.. writers.Where(w => w.Library != "IronXL")];
            Console.WriteLine("[XLBench] Skipping IronXL — XLBENCH_IRONXL_KEY not set (see README).");
        }

        // Telerik watermarks its output when unlicensed (see TelerikLicense).
        if (!TelerikLicense.IsLicensed)
        {
            writers = [.. writers.Where(w => w.Library != "Telerik")];
            Console.WriteLine("[XLBench] Skipping Telerik — no licence found (see README).");
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

                var verification = Verify(InsertOutput.PathFor(slug));
                var lastOk = Matches(result.Total, expected);
                var ok = lastOk && verification.IsClean;
                if (!ok) wrong++;

                Console.WriteLine(
                    $"[XLBench] {library}: {(ok ? "OK" : "MISMATCH")} {lastCell}=" +
                    $"{result.Total.ToString("R", CultureInfo.InvariantCulture)}" +
                    $" — {verification.Describe()}" +
                    $" — {sw.ElapsedMilliseconds:N0} ms (single cold pass, not a benchmark result).");

                if (!lastOk)
                {
                    Console.WriteLine(
                        $"[XLBench]   {lastCell} read back from the library does not match the CSV: expected " +
                        $"{expected.ToString("R", CultureInfo.InvariantCulture)}.");
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

        Console.WriteLine($"[XLBench] Wrote {written} of {writers.Length} insert artifact(s)."
            + (skipped > 0 ? $" {skipped} skipped (file locked — close it in Excel and re-run)." : string.Empty)
            + (failed > 0 ? $" {failed} failed." : string.Empty)
            + (wrong > 0 ? $" {wrong} produced a wrong total." : string.Empty));
    }

    /// <summary>What the saved file says about every data row's total.</summary>
    private sealed record Verification(int Rows, int Checked, int BadValues, int BadFormulas, List<string> Problems)
    {
        public bool IsClean => Checked == Rows && BadValues == 0 && BadFormulas == 0;

        public string Describe() =>
            IsClean
                ? $"{Checked}/{Rows} saved row totals verified"
                : $"{Checked - BadValues}/{Rows} saved row totals verified"
                  + (BadValues > 0 ? $", {BadValues} wrong" : string.Empty)
                  + (Checked < Rows ? $", {Rows - Checked} missing" : string.Empty)
                  + (BadFormulas > 0 ? $", {BadFormulas} not widened" : string.Empty);
    }

    /// <summary>
    /// Reads the <c>SUM</c> column out of the saved package and checks each data row's cached value
    /// against the CSV, and its formula against the range the insert should have widened it to.
    /// Only the first few problems are reported; the counts cover the rest.
    /// </summary>
    private static Verification Verify(string path)
    {
        const int maxReported = 3;

        var column = SavedSheet.ReadColumn(path, InsertData.SumCol);

        var rows = InsertData.RowCount;
        var seen = 0;
        var badValues = 0;
        var badFormulas = 0;
        var problems = new List<string>();

        for (var row = InsertData.FirstDataRow; row <= InsertData.LastDataRow; row++)
        {
            var reference = $"{InsertData.SumColLetter}{row}";
            var expected = InsertData.ExpectedTotal(row - InsertData.FirstDataRow);

            if (!column.TryGetValue(row, out var cell))
            {
                if (problems.Count < maxReported)
                    problems.Add($"{reference} is absent from the saved sheet.");
                continue;
            }

            seen++;

            if (cell.Number is not { } actual)
            {
                badValues++;
                if (problems.Count < maxReported)
                {
                    problems.Add($"{reference} has no usable cached value" +
                                 $" (t=\"{cell.Type ?? "n"}\", v={cell.RawValue ?? "<none>"}).");
                }
            }
            else if (!Matches(actual, expected))
            {
                badValues++;
                if (problems.Count < maxReported)
                {
                    problems.Add(
                        $"{reference} = {actual.ToString("R", CultureInfo.InvariantCulture)}," +
                        $" expected {expected.ToString("R", CultureInfo.InvariantCulture)}" +
                        $" (formula: {cell.Formula ?? "<none>"}).");
                }
            }

            var expectedFormula = InsertData.SumFormula(row);
            if (!string.Equals(cell.Formula, expectedFormula, StringComparison.OrdinalIgnoreCase))
            {
                badFormulas++;
                if (problems.Count < maxReported)
                {
                    problems.Add($"{reference} formula is '{cell.Formula ?? "<none>"}'," +
                                 $" expected '{expectedFormula}'.");
                }
            }
        }

        return new Verification(rows, seen, badValues, badFormulas, problems);
    }

    private static bool Matches(double actual, double expected) =>
        double.IsFinite(actual)
        && Math.Abs(actual - expected) <= Tolerance * Math.Max(1d, Math.Abs(expected));
}
