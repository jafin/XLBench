using XLBench.Benchmarks.Report;

namespace XLBench.Data;

/// <summary>
/// Writes every library's stock-report workbook to <c>output/</c> without running the
/// benchmark harness (<c>dotnet run -- report</c>).
///
/// The benchmarks save the same artifacts from <c>[GlobalCleanup]</c>, so a normal run already
/// refreshes them; this exists so the workbooks can be regenerated and eyeballed in seconds
/// after a change, rather than sitting through a measured run.
/// </summary>
public static class ReportArtifacts
{
    public static void WriteAll()
    {
        Console.WriteLine($"[XLBench] Writing report artifacts to {ReportOutput.Directory}");

        // Each entry is a library's [GlobalCleanup] save, invoked directly. One failing library
        // must not stop the others — a partial set is still reviewable, and the exception text
        // is the useful output. Each returns false when the target file was locked and skipped.
        (string Library, Func<bool> Save)[] writers =
        [
            ("ClosedXML", ClosedXmlReportBenchmarks.WriteArtifact),
            ("EPPlus", EpPlusReportBenchmarks.WriteArtifact),
            ("NPOI", NpoiReportBenchmarks.WriteArtifact),
            ("OpenXML SDK", OpenXmlReportBenchmarks.WriteArtifact),
            ("XLibur", XLiburReportBenchmarks.WriteArtifact),
        ];

        var written = 0;
        var skipped = 0;
        var failed = 0;
        foreach (var (library, save) in writers)
        {
            try
            {
                if (save()) written++;
                else skipped++;
            }
#pragma warning disable S2221 // A broken library must not prevent the others from producing artifacts
            catch (Exception ex)
#pragma warning restore S2221
            {
                failed++;
                Console.WriteLine($"[XLBench] {library} failed: {ex}");
            }
        }

        Console.WriteLine($"[XLBench] Wrote {written} of {writers.Length} report artifact(s)."
            + (skipped > 0 ? $" {skipped} skipped (file locked — close it in Excel and re-run)." : string.Empty)
            + (failed > 0 ? $" {failed} failed." : string.Empty));
    }
}
