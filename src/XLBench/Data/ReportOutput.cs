namespace XLBench.Data;

/// <summary>
/// Persists the workbook each <c>CreateStockReport</c> benchmark builds so the result can be
/// opened and reviewed after the run. Files land in <c>output/</c> at the repo root and are
/// overwritten on the next run.
///
/// The save deliberately happens in <c>[GlobalCleanup]</c> — i.e. once, outside every measured
/// iteration — by re-running the same build delegate against a <see cref="FileStream"/>. Writing
/// to disk inside the benchmark would fold file I/O and antivirus jitter into the timings.
/// </summary>
public static class ReportOutput
{
    /// <summary>
    /// Resolves the output directory: <c>XLBENCH_OUTPUT</c> if set (the host process exports it
    /// so BenchmarkDotNet's child processes inherit a stable path), otherwise a repo-root
    /// <c>output/</c> located by walking up for the solution file, otherwise CWD-relative.
    /// </summary>
    public static string Directory =>
        Environment.GetEnvironmentVariable("XLBENCH_OUTPUT") is { Length: > 0 } configured
            ? configured
            : Path.Combine(RepoRootOrCurrent(), "output");

    /// <summary>Builds the workbook once more, unmeasured, and writes it to <c>output/stock-report-{slug}.xlsx</c>.</summary>
    /// <param name="slug">Lower-case library identifier used in the file name, e.g. "epplus".</param>
    /// <param name="build">The same delegate the benchmark measures, targeting an arbitrary stream.</param>
    /// <returns><c>true</c> if the file was written; <c>false</c> if it was locked and skipped.</returns>
    public static bool Save(string slug, Action<Stream> build)
    {
        var dir = Directory;
        var path = Path.Combine(dir, $"stock-report-{slug}.xlsx");

        try
        {
            System.IO.Directory.CreateDirectory(dir);
            // ReadWrite, not Write: the OpenXML SDK's package writer reads back from the stream
            // it is given, and throws "The stream was not opened for reading" on a write-only one.
            using var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            build(fs);
            Console.WriteLine($"[XLBench] Saved {path}");
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Most likely the previous artifact is still open in Excel. Losing the artifact is
            // not worth failing an otherwise-good benchmark run over.
            Console.WriteLine($"[XLBench] Could not write {path}: {ex.Message}");
            return false;
        }
    }

    private static string RepoRootOrCurrent()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (dir.EnumerateFiles("XLBench.slnx").Any())
                return dir.FullName;
        }
        return System.IO.Directory.GetCurrentDirectory();
    }
}
