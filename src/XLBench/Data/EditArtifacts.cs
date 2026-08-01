using System.Diagnostics;
using System.Globalization;
using XLBench.Benchmarks.Edit;
using XLBench.Libraries;

namespace XLBench.Data;

/// <summary>
/// Runs every library's <c>EditAndRecalculate</c> once, unmeasured, writing the edited workbook
/// to <c>output/</c> and checking the recalculated total against the value computed straight from
/// the CSV (<c>dotnet run -- edit</c>).
///
/// The benchmarks save the same artifacts from <c>[GlobalCleanup]</c>, so a normal run already
/// refreshes them; this exists so the scenario can be verified and eyeballed in seconds after a
/// change, rather than sitting through a measured run. The elapsed time it prints is a single
/// cold pass — indicative only, never a benchmark result.
/// </summary>
public static class EditArtifacts
{
    /// <summary>
    /// Relative tolerance for the recalculated total. The libraries sum the same twenty doubles
    /// but not necessarily in the same order, so the last few ulps are expected to differ.
    /// </summary>
    private const double Tolerance = 1e-9;

    public static void WriteAll()
    {
        EditData.EnsureLoaded();

        var expected = EditData.ExpectedLastTotal;
        var lastCell = $"{EditData.SumColLetter}{EditData.LastRowAfterEdit}";

        Console.WriteLine(
            $"[XLBench] Edit scenario: {EditData.RowCount} rows x {EditData.ColCount} columns; " +
            $"deleting {EditData.RowsToDelete.Length} rows leaves {EditData.SurvivingRowCount}.");
        Console.WriteLine(
            $"[XLBench] Expected {lastCell} after the edit: {expected.ToString("R", CultureInfo.InvariantCulture)}");
        Console.WriteLine($"[XLBench] Writing edit artifacts to {EditOutput.Directory}");

        // Each entry is a library's [GlobalCleanup] save, invoked directly. One failing library
        // must not stop the others — a partial set is still reviewable, and the exception text
        // is the useful output.
        (string Library, Func<EditResult> Run)[] writers =
        [
            ("ClosedXML", ClosedXmlEditBenchmarks.WriteArtifact),
            ("EPPlus", EpPlusEditBenchmarks.WriteArtifact),
            ("NPOI", NpoiEditBenchmarks.WriteArtifact),
            ("OpenXML SDK", OpenXmlEditBenchmarks.WriteArtifact),
            ("XLibur", XLiburEditBenchmarks.WriteArtifact),
            ("IronXL", IronXlEditBenchmarks.WriteArtifact),
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

        foreach (var (library, run) in writers)
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
                var ok = Matches(result.Total, expected);
                if (!ok) wrong++;

                Console.WriteLine(
                    $"[XLBench] {library}: {(ok ? "OK" : "MISMATCH")} {lastCell}=" +
                    $"{result.Total.ToString("R", CultureInfo.InvariantCulture)}" +
                    $" — {sw.ElapsedMilliseconds:N0} ms (single cold pass, not a benchmark result).");
            }
#pragma warning disable S2221 // A broken library must not prevent the others from producing artifacts
            catch (Exception ex)
#pragma warning restore S2221
            {
                failed++;
                Console.WriteLine($"[XLBench] {library} failed: {ex}");
            }
        }

        Console.WriteLine($"[XLBench] Wrote {written} of {writers.Length} edit artifact(s)."
            + (skipped > 0 ? $" {skipped} skipped (file locked — close it in Excel and re-run)." : string.Empty)
            + (failed > 0 ? $" {failed} failed." : string.Empty)
            + (wrong > 0 ? $" {wrong} produced the wrong total." : string.Empty));
    }

    private static bool Matches(double actual, double expected) =>
        double.IsFinite(actual)
        && Math.Abs(actual - expected) <= Tolerance * Math.Max(1d, Math.Abs(expected));
}
