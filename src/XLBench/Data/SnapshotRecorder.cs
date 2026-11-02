using System.Globalization;
using BenchmarkDotNet.Reports;

namespace XLBench.Data;

/// <summary>
/// Writes a library's measured results into <c>snapshots/</c> so that <see cref="LibrarySnapshot"/>
/// can replay them in a later run, plus the markdown helpers the main run shares with it.
/// </summary>
/// <remarks>
/// IronXL cannot share a package graph with the other libraries (see <c>XLBench.IronXL</c>), so
/// it is measured by its own project, in its own process, and that project calls
/// <see cref="Save"/> once the run finishes. The main project only ever replays the snapshot.
/// </remarks>
internal static class SnapshotRecorder
{
    /// <summary>Snapshots every report in <paramref name="summaries"/> as <paramref name="library"/>.</summary>
    public static int Save(string library, string version, IEnumerable<Summary> summaries)
    {
        var summaryList = summaries.ToList();
        var host = HostOf(summaryList);
        var job = summaryList
            .SelectMany(s => s.BenchmarksCases)
            .Select(c => c.Job.ResolvedId)
            .FirstOrDefault() ?? string.Empty;
        var capturedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);

        // Pair each measured method with the markdown row BenchmarkDotNet rendered for it,
        // so a later run can reproduce the same table cells rather than inventing them.
        var (header, rowsByMethod) = ExtractMarkdownRows(summaryList, library);

        var entries = new Dictionary<string, LibrarySnapshot.SnapshotEntry>();
        foreach (var report in summaryList.SelectMany(s => s.Reports))
        {
            var method = report.BenchmarkCase.Descriptor.WorkloadMethod.Name;

            // All statistics are in nanoseconds. They can be NaN/Infinity for degenerate runs;
            // Finite() maps those to null so JSON serialization cannot fail.
            var stats = report.ResultStatistics;
            var allocMetric = report.Metrics.Values
                .FirstOrDefault(m => m.Descriptor.Id.Contains("Allocated", StringComparison.OrdinalIgnoreCase));

            var timeMs = Finite(stats?.Mean is { } ns ? ns / 1_000_000.0 : null);
            if (timeMs is null) continue;

            entries[method] = new LibrarySnapshot.SnapshotEntry
            {
                Version = version,
                CapturedUtc = capturedUtc,
                Host = host,
                Job = job,
                TimeMs = timeMs,
                AllocMb = Finite(allocMetric is not null ? allocMetric.Value / (1024.0 * 1024.0) : null),
                ErrorMs = Finite(stats is not null ? stats.ConfidenceInterval.Margin / 1_000_000.0 : null),
                StdDevMs = Finite(stats is not null ? stats.StandardDeviation / 1_000_000.0 : null),
                TableHeader = header,
                TableRow = rowsByMethod.GetValueOrDefault(method),
            };
        }

        if (entries.Count > 0)
            LibrarySnapshot.Merge(library, entries);
        return entries.Count;
    }

    /// <summary>Maps NaN/Infinity (which are not valid JSON) to null; passes finite values through.</summary>
    private static double? Finite(double? v) => v is { } d && double.IsFinite(d) ? d : null;

    public static string HostOf(IEnumerable<Summary> summaries) =>
        summaries
            .Select(s => $"{s.HostEnvironmentInfo.Os.Value}, {s.HostEnvironmentInfo.Cpu.Value?.ProcessorName}")
            .FirstOrDefault() ?? string.Empty;

    public static string[] SplitRow(string line) =>
        line.Trim().Trim('|').Split('|').Select(c => c.Trim()).ToArray();

    /// <summary>
    /// Pulls a library's rendered markdown rows out of the generated GitHub reports, keyed by
    /// method, along with the header they belong to.
    /// </summary>
    private static (string? Header, Dictionary<string, string> RowsByMethod) ExtractMarkdownRows(
        IEnumerable<Summary> summaries, string library)
    {
        var rowsByMethod = new Dictionary<string, string>();
        string? header = null;

        var resultsDir = summaries.Select(s => s.ResultsDirectoryPath).FirstOrDefault(Directory.Exists);
        if (resultsDir is null) return (null, rowsByMethod);

        foreach (var file in Directory.GetFiles(resultsDir, "*-report-github.md").OrderBy(f => f))
        {
            var lines = File.ReadAllText(file).Replace("\r\n", "\n").Split('\n');
            var headerIdx = Array.FindIndex(lines, l =>
            {
                var t = l.TrimStart();
                return t.StartsWith('|') && l.Contains("Method") && l.Contains("Mean");
            });
            if (headerIdx < 0) continue;

            var headers = SplitRow(lines[headerIdx]);
            var libraryCol = Array.FindIndex(headers, c => c.Equals("Library", StringComparison.OrdinalIgnoreCase));
            var methodCol = Array.FindIndex(headers, c => c.Equals("Method", StringComparison.OrdinalIgnoreCase));
            if (libraryCol < 0 || methodCol < 0) continue;

            header ??= lines[headerIdx];
            for (var i = headerIdx + 2; i < lines.Length && lines[i].TrimStart().StartsWith('|'); i++)
            {
                var cells = SplitRow(lines[i]);
                if (cells.ElementAtOrDefault(libraryCol) == library &&
                    cells.ElementAtOrDefault(methodCol) is { Length: > 0 } method)
                {
                    rowsByMethod[method] = lines[i];
                }
            }
        }

        return (header, rowsByMethod);
    }
}
