using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using XLBench;
using XLBench.Config;
using XLBench.Data;

// Run all (or CLI-filtered) benchmarks with the shared comparison config, then publish
// the generated GitHub-flavored markdown into ./docs for GitHub Pages.
//
// Examples:
//   dotnet run -c Release                       run everything
//   dotnet run -c Release -- --filter *Read*     only read benchmarks
//   dotnet run -c Release -- --job short         faster, lower-fidelity run
// `dotnet run -- versions` prints the resolved library versions and exits (no benchmarks).
if (args is [{ } first, ..] && first.Equals("versions", StringComparison.OrdinalIgnoreCase))
{
    foreach (var (lib, ver) in LibraryVersions.All)
        Console.WriteLine($"{lib}\t{ver}");
    return;
}

// `dotnet run -- report` writes just the report-scenario .xlsx artifacts to output/ and exits.
// Same code path the benchmarks measure, without the (slow) measurement — handy for
// regenerating the reviewable workbooks after a change.
if (args is [{ } cmd, ..] && cmd.Equals("report", StringComparison.OrdinalIgnoreCase))
{
    ReportArtifacts.WriteAll();
    return;
}

// `dotnet run -- edit` does the same for the edit scenario, and additionally checks each
// library's recalculated row total against the value computed straight from numbers.csv.
if (args is [{ } editCmd, ..] && editCmd.Equals("edit", StringComparison.OrdinalIgnoreCase))
{
    EditArtifacts.WriteAll();
    return;
}

// `dotnet run -- insert` does the same for the insert scenario. It checks more than the edit
// command does: every row total in the *saved* file, not just the one cell the benchmark reads.
if (args is [{ } insertCmd, ..] && insertCmd.Equals("insert", StringComparison.OrdinalIgnoreCase))
{
    InsertArtifacts.WriteAll();
    return;
}

var config = BenchmarkConfig.Create();

// The report benchmarks save their workbook for manual review from [GlobalCleanup], which runs
// inside BenchmarkDotNet's generated child process — where the working directory is the build
// folder, not the repo root. Pin the destination here (child processes inherit the environment)
// so artifacts always land in one predictable place.
Environment.SetEnvironmentVariable("XLBENCH_OUTPUT", XLBench.Data.ReportOutput.Directory);

// BenchmarkDotNet accumulates report files across runs; clear the previous run's results
// so the published docs/results.md reflects only the current run.
ResultsPublisher.CleanPreviousResults();

var summaries = BenchmarkSwitcher
    .FromAssembly(typeof(Program).Assembly)
    .Run(args, config);

ResultsPublisher.Publish(summaries);

namespace XLBench
{
    /// <summary>Marker type used to locate the benchmark assembly.</summary>
    internal sealed class Program;

    /// <summary>
    /// Copies BenchmarkDotNet's generated GitHub markdown reports into the docs/ folder
    /// (published via GitHub Pages), prefixed with a run-metadata header. The docs
    /// directory is resolved from <c>XLBENCH_DOCS</c> if set, otherwise ./docs relative
    /// to the current working directory (the repo root when run via scripts/run-benchmarks.ps1).
    /// </summary>
    internal static class ResultsPublisher
    {
        private static string ArtifactsResultsDir =>
            Path.Combine(Directory.GetCurrentDirectory(), "BenchmarkDotNet.Artifacts", "results");

        /// <summary>Removes report files from a prior run so results.md reflects only this run.</summary>
        public static void CleanPreviousResults()
        {
            if (!Directory.Exists(ArtifactsResultsDir)) return;
            foreach (var f in Directory.GetFiles(ArtifactsResultsDir, "*-report-github.md"))
            {
                try { File.Delete(f); } catch (IOException) { /* best effort */ }
            }
        }

        public static void Publish(IEnumerable<Summary> summaries)
        {
            var summaryList = summaries.ToList();
            if (summaryList.Count == 0)
            {
                Console.WriteLine("[XLBench] No summaries produced; nothing to publish.");
                return;
            }

            var docsDir = Environment.GetEnvironmentVariable("XLBENCH_DOCS")
                          ?? Path.Combine(Directory.GetCurrentDirectory(), "docs");
            Directory.CreateDirectory(docsDir);

            // BenchmarkDotNet writes *-report-github.md under the artifacts results folder.
            var artifactsResults = summaryList
                .Select(s => Path.Combine(s.ResultsDirectoryPath))
                .FirstOrDefault(Directory.Exists);

            if (artifactsResults is null)
            {
                Console.WriteLine("[XLBench] Could not locate BenchmarkDotNet results directory.");
                return;
            }

            var reports = Directory
                .GetFiles(artifactsResults, "*-report-github.md")
                .OrderBy(f => f)
                .ToList();

            if (reports.Count == 0)
            {
                Console.WriteLine($"[XLBench] No *-report-github.md files in {artifactsResults}.");
                return;
            }

            // A configurable output name + optional CI banner let a noisy CI run publish to a
            // separate results-ci.md without clobbering the authoritative results.md or the
            // interactive-chart data.
            var basename = Environment.GetEnvironmentVariable("XLBENCH_RESULTS_BASENAME") ?? "results";
            var ciBanner = Environment.GetEnvironmentVariable("XLBENCH_CI_BANNER");

            // YAML front matter ensures Jekyll processes this page through the theme layout.
            var frontMatter =
                $"""
                ---
                title: XLBench Results{(string.IsNullOrEmpty(ciBanner) ? "" : " (CI)")}
                ---
                """;

            // Widen the Cayman theme's narrow (64rem) content column to 90% so the wide
            // results table fits without a horizontal scrollbar.
            const string styleBlock =
                """
                <style>
                  .main-content { max-width: 90% !important; padding-left: 2rem !important; padding-right: 2rem !important; }
                </style>

                """;

            const string header =
                """
                <!-- Generated by XLBench. Do not edit by hand; re-run scripts/run-benchmarks.ps1. -->
                # XLBench Results

                Each section below embeds the full BenchmarkDotNet environment block (OS, CPU,
                .NET SDK and runtime). Timings are hardware-specific; treat the allocation/GC
                columns as the most portable signal across machines. Regenerate locally with
                `scripts/run-benchmarks.ps1`.


                """;

            var scenarios = ExtractScenarios(summaryList);

            var outFile = Path.Combine(docsDir, $"{basename}.md");
            using (var writer = new StreamWriter(outFile, append: false))
            {
                writer.WriteLine(frontMatter);
                writer.Write(styleBlock);
                writer.Write(header);
                if (!string.IsNullOrEmpty(ciBanner))
                {
                    writer.WriteLine($"> {ciBanner}");
                    writer.WriteLine();
                }
                writer.WriteLine("📈 **[Interactive charts](charts.html)** (GitHub Pages). Static charts and the full tables follow.");
                writer.WriteLine();
                writer.WriteLine("Each results table (one per test method) is heat-mapped independently on the Pages site (🟩 best → 🟥 worst).");
                writer.WriteLine();
                writer.Write(BuildSnapshotNotice(scenarios, HostOf(summaryList)));
                writer.Write(BuildVersionsTable());
                writer.Write(BuildMermaidSection(scenarios));
                writer.WriteLine("## Detailed results");
                writer.WriteLine();
                foreach (var report in reports)
                {
                    writer.WriteLine();
                    // Split BenchmarkDotNet's single joined table into one table per test method
                    // so each is compared (and heat-mapped) like-for-like.
                    writer.Write(BuildSplitTables(File.ReadAllText(report)));
                    writer.WriteLine();
                }

                // GitHub Pages' Jekyll theme has no mermaid.js, so the ```mermaid fences
                // render as plain code. Load mermaid client-side and render them. (On
                // github.com the fences render natively, so this script is simply ignored.)
                writer.WriteLine();
                writer.Write(MermaidRenderScript);

                // Heat-map the numeric results-table cells per scenario (client-side, Pages only).
                writer.WriteLine();
                writer.Write(HeatmapScript);
            }

            // Only the authoritative page refreshes the interactive-chart data; a CI run
            // (separate basename) must not overwrite it with noisy numbers.
            if (basename == "results")
                WriteChartData(docsDir, scenarios);

            Console.WriteLine($"[XLBench] Published {Path.GetFileName(outFile)} ({reports.Count} report(s), {scenarios.Count} scenario(s)) to {docsDir}");
        }

        // Client-side mermaid loader for the GitHub Pages site. Converts Jekyll's rendered
        // ```mermaid code blocks into <pre class="mermaid"> and renders them.
        private const string MermaidRenderScript =
            """
            <script type="module">
              import mermaid from 'https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.esm.min.mjs';
              const blocks = document.querySelectorAll('.language-mermaid');
              blocks.forEach(el => {
                const code = el.querySelector('code') || el;
                const pre = document.createElement('pre');
                pre.className = 'mermaid';
                pre.textContent = code.textContent;
                el.replaceWith(pre);
              });
              if (blocks.length) {
                mermaid.initialize({ startOnLoad: false });
                await mermaid.run({ querySelector: 'pre.mermaid' });
              }
            </script>

            """;

        // Heat-maps the numeric metric columns (Mean, Gen0/1/2, Allocated) of every results
        // table. Each table is a single test method, so colouring per-table is inherently
        // per-scenario: green = best, red = worst.
        private const string HeatmapScript =
            """
            <script>
            (function () {
              const parse = s => { const n = parseFloat(String(s).replace(/,/g, '')); return Number.isFinite(n) ? n : null; };
              document.querySelectorAll('table').forEach(table => {
                if (!table.tHead || !table.tBodies[0]) return;
                const heads = [...table.tHead.rows[0].cells].map(c => c.textContent.trim().toLowerCase());
                if (!heads.includes('mean')) return;
                const metricCols = heads.map((h, i) => /^(mean|allocated|gen0|gen1|gen2)$/.test(h) ? i : -1).filter(i => i >= 0);
                const rows = [...table.tBodies[0].rows];
                for (const c of metricCols) {
                  const vals = rows.map(r => parse(r.cells[c]?.textContent));
                  const nums = vals.filter(v => v !== null);
                  if (nums.length < 2) continue;
                  const min = Math.min(...nums), max = Math.max(...nums);
                  if (max === min) continue;
                  rows.forEach((r, i) => {
                    const v = vals[i]; if (v === null) return;
                    const ratio = (v - min) / (max - min);
                    r.cells[c].style.backgroundColor = `hsla(${120 - 120 * ratio}, 70%, 50%, 0.45)`;
                  });
                }
              });
            })();
            </script>

            """;

        // ---- Metric extraction -------------------------------------------------------

        private sealed record Series(
            string Library, double? TimeMs, double? AllocMb, double? ErrorMs, double? StdDevMs,
            string? SnapshotOf = null);

        /// <summary>Maps NaN/Infinity (which are not valid JSON) to null; passes finite values through.</summary>
        private static double? Finite(double? v) => v is { } d && double.IsFinite(d) ? d : null;

        private sealed record Scenario(string Key, string Label, IReadOnlyList<Series> ByLibrary);

        // Benchmark method name -> human label. Order controls chart order.
        private static readonly (string Key, string Label)[] MethodLabels =
        [
            ("OpenWorkbook", "Read · open workbook"),
            ("OpenAndReadAll", "Read · open + read all cells"),
            ("CreateAndSave", "Write · create + save"),
            ("CreateStockReport", "Report · data + conditional formatting + chart"),
            ("EditAndRecalculate", "Edit · delete rows + set column + recalculate"),
            ("InsertColumnsAndRecalculate", "Edit · insert 2 columns + recalculate"),
        ];

        /// <summary>
        /// Libraries that cannot run without external credentials, so their results are carried
        /// between runs in <c>snapshots/</c>. See <see cref="LibrarySnapshot"/>.
        /// </summary>
        private static readonly string[] SnapshotLibraries = ["IronXL"];

        private static List<Scenario> ExtractScenarios(IEnumerable<Summary> summaries)
        {
            SnapshotRows.Clear();

            // Flatten every report into (library, method, time, allocated).
            var rows = new List<(string Library, string Method, double? TimeMs, double? AllocMb, double? ErrorMs, double? StdDevMs, string? SnapshotOf)>();
            foreach (var summary in summaries)
            foreach (var report in summary.Reports)
            {
                var lib = LibraryNames.FromTypeName(report.BenchmarkCase.Descriptor.Type.Name);
                var method = report.BenchmarkCase.Descriptor.WorkloadMethod.Name;

                // All statistics are in nanoseconds; the interactive page works in ms.
                // Stats can be NaN/Infinity for degenerate runs (e.g. a single measurement on a
                // noisy CI runner); Finite() maps those to null so JSON serialization can't fail.
                var stats = report.ResultStatistics;
                double? timeMs = Finite(stats?.Mean is { } ns ? ns / 1_000_000.0 : null);
                // "Error" matches BenchmarkDotNet's Error column: the 99.9% CI half-width.
                double? errorMs = Finite(stats is not null ? stats.ConfidenceInterval.Margin / 1_000_000.0 : null);
                double? stdDevMs = Finite(stats is not null ? stats.StandardDeviation / 1_000_000.0 : null);

                var allocMetric = report.Metrics.Values
                    .FirstOrDefault(m => m.Descriptor.Id.Contains("Allocated", StringComparison.OrdinalIgnoreCase));
                double? allocMb = Finite(allocMetric is not null ? allocMetric.Value / (1024.0 * 1024.0) : null);

                rows.Add((lib, method, timeMs, allocMb, errorMs, stdDevMs, null));
            }

            // A library that ran this time replaces its snapshot; one that did not is restored
            // from it, so the comparison stays complete across runs that lack a licence key.
            foreach (var library in SnapshotLibraries)
            {
                if (rows.Any(r => r.Library == library))
                    SaveSnapshot(library, summaries, rows.Where(r => r.Library == library));
                else
                    rows.AddRange(RestoreSnapshot(library));
            }

            var scenarios = new List<Scenario>();
            foreach (var (key, label) in MethodLabels)
            {
                var byLibrary = rows
                    .Where(r => r.Method == key && r.TimeMs.HasValue)
                    .OrderBy(r => r.TimeMs)
                    .Select(r => new Series(r.Library, r.TimeMs, r.AllocMb, r.ErrorMs, r.StdDevMs, r.SnapshotOf))
                    .ToList();
                if (byLibrary.Count > 0)
                    scenarios.Add(new Scenario(key, label, byLibrary));
            }
            return scenarios;
        }

        // ---- Snapshots for licence-gated libraries -----------------------------------

        /// <summary>Rows replayed from a snapshot, keyed for merging back into the live set.</summary>
        private static IEnumerable<(string Library, string Method, double? TimeMs, double? AllocMb, double? ErrorMs, double? StdDevMs, string? SnapshotOf)>
            RestoreSnapshot(string library)
        {
            var snapshot = LibrarySnapshot.Load(library);
            if (snapshot is null) yield break;

            Console.WriteLine(
                $"[XLBench] {library} did not run; replaying {snapshot.Methods.Count} snapshotted result(s) " +
                $"from {LibrarySnapshot.PathFor(library)}.");

            foreach (var (method, entry) in snapshot.Methods)
            {
                SnapshotRows[(library, method)] = entry;
                yield return (library, method, entry.TimeMs, entry.AllocMb, entry.ErrorMs,
                    entry.StdDevMs, LibrarySnapshot.Describe(entry));
            }
        }

        /// <summary>Snapshot entries in play for this publish, so the tables can replay their rows.</summary>
        private static readonly Dictionary<(string Library, string Method), LibrarySnapshot.SnapshotEntry> SnapshotRows = [];

        private static void SaveSnapshot(
            string library,
            IEnumerable<Summary> summaries,
            IEnumerable<(string Library, string Method, double? TimeMs, double? AllocMb, double? ErrorMs, double? StdDevMs, string? SnapshotOf)> measured)
        {
            var summaryList = summaries.ToList();
            var host = HostOf(summaryList);
            var job = summaryList
                .SelectMany(s => s.BenchmarksCases)
                .Select(c => c.Job.ResolvedId)
                .FirstOrDefault() ?? string.Empty;
            var version = LibraryVersions.For(library);
            var capturedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);

            // Pair each measured method with the markdown row BenchmarkDotNet rendered for it,
            // so a later keyless run can reproduce the same table cells rather than inventing them.
            var (header, rowsByMethod) = ExtractMarkdownRows(summaryList, library);

            var entries = new Dictionary<string, LibrarySnapshot.SnapshotEntry>();
            foreach (var r in measured)
            {
                entries[r.Method] = new LibrarySnapshot.SnapshotEntry
                {
                    Version = version,
                    CapturedUtc = capturedUtc,
                    Host = host,
                    Job = job,
                    TimeMs = r.TimeMs,
                    AllocMb = r.AllocMb,
                    ErrorMs = r.ErrorMs,
                    StdDevMs = r.StdDevMs,
                    TableHeader = header,
                    TableRow = rowsByMethod.GetValueOrDefault(r.Method),
                };
            }

            LibrarySnapshot.Merge(library, entries);
        }

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

        // ---- Library versions --------------------------------------------------------

        private static string BuildVersionsTable()
        {
            var sb = new StringBuilder();
            sb.AppendLine("## Libraries under test").AppendLine();
            sb.AppendLine("| Library | Version | Source |");
            sb.AppendLine("| --- | --- | --- |");
            foreach (var (lib, ver) in LibraryVersions.All)
                sb.AppendLine($"| {lib} | {ver} | this run |");

            // Snapshotted libraries did not run, so report the version their numbers came from
            // rather than the version currently referenced.
            foreach (var library in SnapshotLibraries)
            {
                var entry = SnapshotRows
                    .Where(kv => kv.Key.Library == library)
                    .Select(kv => kv.Value)
                    .FirstOrDefault();
                if (entry is not null)
                    sb.AppendLine($"| {library} {SnapshotMarker} | {entry.Version} | snapshot ({LibrarySnapshot.Describe(entry)}) |");
            }

            sb.AppendLine();
            return sb.ToString();
        }

        /// <summary>
        /// Explains the <c>⧗</c> marker whenever any published number was replayed from
        /// <c>snapshots/</c> instead of measured in this run.
        /// </summary>
        private static string HostOf(IEnumerable<Summary> summaries) =>
            summaries
                .Select(s => $"{s.HostEnvironmentInfo.Os.Value}, {s.HostEnvironmentInfo.Cpu.Value?.ProcessorName}")
                .FirstOrDefault() ?? string.Empty;

        private static string BuildSnapshotNotice(IReadOnlyList<Scenario> scenarios, string currentHost)
        {
            var libraries = scenarios
                .SelectMany(s => s.ByLibrary)
                .Where(b => b.SnapshotOf is not null)
                .Select(b => b.Library)
                .Distinct()
                .ToList();

            if (libraries.Count == 0) return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine(
                $"> {SnapshotMarker} **Carried over from an earlier run.** The rows and chart points below " +
                "marked with this symbol were not measured here — the library is commercial and needs a " +
                "licence key this run did not have — so they are replayed from `snapshots/`. Compare them " +
                "as indicative rather than like-for-like, and see the repository README for how to refresh " +
                "a snapshot.");
            sb.AppendLine(">");

            foreach (var library in libraries)
            {
                var entry = SnapshotRows.First(kv => kv.Key.Library == library).Value;

                // State the hardware rather than assume it differs — a snapshot refreshed on this
                // same machine is directly comparable, and saying otherwise would undersell it.
                var hardware = string.Equals(entry.Host, currentHost, StringComparison.Ordinal)
                    ? "same hardware as this run"
                    : $"different hardware — {entry.Host}";

                sb.AppendLine($"> - **{library}** {entry.Version}, {entry.Job} job, "
                              + $"captured {Captured(entry)} on {hardware}.");
            }

            sb.AppendLine();
            return sb.ToString();
        }

        private static string Captured(LibrarySnapshot.SnapshotEntry entry) =>
            DateTime.TryParse(entry.CapturedUtc, CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind, out var d)
                ? d.ToUniversalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                : "an unknown date";

        // ---- Split the joined table into one table per test method -------------------

        private static string BuildSplitTables(string report)
        {
            var lines = report.Replace("\r\n", "\n").Split('\n');

            var headerIdx = Array.FindIndex(lines, l =>
            {
                var t = l.TrimStart();
                return t.StartsWith('|') && l.Contains("Method") && l.Contains("Mean");
            });
            if (headerIdx < 1) return report; // unexpected format — leave unchanged

            var headerLine = lines[headerIdx];
            var delimLine = lines[headerIdx + 1];
            var headers = SplitRow(headerLine);
            var methodCol = Array.FindIndex(headers, c => c.Equals("Method", StringComparison.OrdinalIgnoreCase));
            var meanCol = Array.FindIndex(headers, c => c.Equals("Mean", StringComparison.OrdinalIgnoreCase));
            var libraryCol = Array.FindIndex(headers, c => c.Equals("Library", StringComparison.OrdinalIgnoreCase));
            if (methodCol < 0) return report;

            var dataRows = new List<string>();
            for (var i = headerIdx + 2; i < lines.Length && lines[i].TrimStart().StartsWith('|'); i++)
                dataRows.Add(lines[i]);

            // Everything before the table (the fenced environment block) is kept once, up top.
            var preamble = string.Join("\n", lines.Take(headerIdx)).TrimEnd('\n');

            var byMethod = dataRows
                .GroupBy(r => SplitRow(r).ElementAtOrDefault(methodCol) ?? string.Empty)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Replay any snapshotted rows for libraries that could not run (see LibrarySnapshot).
            // A snapshot is by definition a row from a different run, so its cells are matched to
            // this run's columns *by header name* rather than by position — BenchmarkDotNet re-pads
            // cell widths every run, and picks some columns per summary (Median only appears for
            // certain distributions), so the raw header strings rarely match even for the same job.
            // Two things can still make a row unusable, and both drop it rather than put wrong
            // numbers under right-looking headings: a column this run renders that the snapshot
            // never captured, and a Mean recorded in a different unit, since units are per-table.
            foreach (var ((library, method), entry) in SnapshotRows)
            {
                if (entry.TableRow is not { Length: > 0 } row) continue;
                if (entry.TableHeader is not { } storedHeader) continue;
                if (!byMethod.TryGetValue(method, out var list)) continue;

                if (ProjectOntoColumns(row, storedHeader, headers) is not { } projected)
                {
                    Console.WriteLine(
                        $"[XLBench] Omitting snapshotted {library}.{method} row from the table: this run " +
                        "reports a column the snapshot never captured (usually a different --job). " +
                        "The chart data still uses it.");
                    continue;
                }
                row = projected;

                var liveUnit = UnitOf(SplitRow(list[0]).ElementAtOrDefault(meanCol));
                var snapshotUnit = UnitOf(SplitRow(row).ElementAtOrDefault(meanCol));
                if (meanCol >= 0 && liveUnit is not null && snapshotUnit is not null && liveUnit != snapshotUnit)
                {
                    Console.WriteLine(
                        $"[XLBench] Omitting snapshotted {library}.{method} row from the table: " +
                        $"recorded in {snapshotUnit}, this run reports {liveUnit}. The chart data still uses it.");
                    continue;
                }

                list.Add(MarkSnapshotRow(row, libraryCol));
            }

            // Known methods first (in MethodLabels order), then any others.
            var orderedKeys = MethodLabels.Select(m => m.Key).Where(byMethod.ContainsKey)
                .Concat(byMethod.Keys.Where(k => MethodLabels.All(m => m.Key != k)))
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine(preamble).AppendLine();
            foreach (var key in orderedKeys)
            {
                var label = MethodLabels.FirstOrDefault(m => m.Key == key).Label ?? key;
                var rows = byMethod[key];
                if (meanCol >= 0)
                    rows = rows.OrderBy(r => ParseLeadingNumber(SplitRow(r).ElementAtOrDefault(meanCol))).ToList();

                sb.AppendLine($"### {label}").AppendLine();
                sb.AppendLine(headerLine);
                sb.AppendLine(delimLine);
                foreach (var r in rows) sb.AppendLine(r);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static string[] SplitRow(string line) =>
            line.Trim().Trim('|').Split('|').Select(c => c.Trim()).ToArray();

        /// <summary>
        /// Re-orders a snapshotted row to match <paramref name="targetHeaders"/>, matching cells by
        /// header name. Columns the snapshot captured but this run does not render (typically
        /// Median, which BenchmarkDotNet only emits for some distributions) are dropped; a column
        /// this run renders that the snapshot lacks returns <c>null</c>, since the only ways to
        /// fill it would be to invent a value or to silently shift every later cell.
        /// </summary>
        private static string? ProjectOntoColumns(string row, string storedHeader, string[] targetHeaders)
        {
            var storedHeaders = SplitRow(storedHeader);
            var storedCells = SplitRow(row);

            var projected = new string[targetHeaders.Length];
            for (var i = 0; i < targetHeaders.Length; i++)
            {
                var source = Array.FindIndex(storedHeaders,
                    h => h.Equals(targetHeaders[i], StringComparison.OrdinalIgnoreCase));
                if (source < 0 || source >= storedCells.Length) return null;
                projected[i] = storedCells[source];
            }

            return $"| {string.Join(" | ", projected)} |";
        }

        /// <summary>
        /// Tags a replayed row's Library cell so a snapshot is never mistaken for a live
        /// measurement. Falls back to a leading marker if the Library column is absent.
        /// </summary>
        private static string MarkSnapshotRow(string row, int libraryCol)
        {
            var cells = SplitRow(row);
            if (libraryCol < 0 || libraryCol >= cells.Length)
                return row;

            cells[libraryCol] = $"{cells[libraryCol]} {SnapshotMarker}";
            return $"| {string.Join(" | ", cells)} |";
        }

        private const string SnapshotMarker = "⧗";

        /// <summary>Trailing unit of a BenchmarkDotNet numeric cell, e.g. "ms" from "1,662.9 ms".</summary>
        private static string? UnitOf(string? cell)
        {
            if (string.IsNullOrWhiteSpace(cell)) return null;
            var m = Regex.Match(cell.Trim(), @"[\d.,]+\s*([^\s\d.,]+)$");
            return m.Success ? m.Groups[1].Value : null;
        }

        private static double ParseLeadingNumber(string? cell)
        {
            if (cell is null) return double.MaxValue;
            var m = Regex.Match(cell.Replace(",", string.Empty), @"-?\d+(\.\d+)?");
            return m.Success && double.TryParse(m.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var v)
                ? v : double.MaxValue;
        }

        // ---- Mermaid (static, renders in the GitHub file view) -----------------------

        private static string BuildMermaidSection(IReadOnlyList<Scenario> scenarios)
        {
            if (scenarios.Count == 0) return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine("## Comparison charts").AppendLine();
            sb.AppendLine("_Lower is better. Charts render in the GitHub file view; the Pages site has interactive versions._").AppendLine();

            foreach (var s in scenarios)
            {
                AppendMermaidBar(sb, $"{s.Label} — time (ms)", "Time (ms)",
                    s.ByLibrary.Select(b => (ChartLabel(b), b.TimeMs)));

                var withAlloc = s.ByLibrary.Where(b => b.AllocMb.HasValue)
                    .OrderBy(b => b.AllocMb).Select(b => (ChartLabel(b), b.AllocMb)).ToList();
                if (withAlloc.Count > 0)
                    AppendMermaidBar(sb, $"{s.Label} — allocated (MB)", "Allocated (MB)", withAlloc);
            }
            return sb.ToString();
        }

        /// <summary>Chart axis label, marked when the value came from a snapshot rather than this run.</summary>
        private static string ChartLabel(Series s) =>
            s.SnapshotOf is null ? s.Library : $"{s.Library} {SnapshotMarker}";

        private static void AppendMermaidBar(StringBuilder sb, string title, string yAxis,
            IEnumerable<(string Library, double? Value)> data)
        {
            var points = data.Where(d => d.Value.HasValue).ToList();
            if (points.Count == 0) return;

            var labels = string.Join(", ", points.Select(p => $"\"{p.Library}\""));
            var values = string.Join(", ", points.Select(p =>
                Math.Round(p.Value!.Value, 2).ToString(CultureInfo.InvariantCulture)));

            sb.AppendLine("```mermaid");
            sb.AppendLine("---");
            sb.AppendLine("config:");
            sb.AppendLine("  xyChart:");
            sb.AppendLine("    showDataLabel: true");
            sb.AppendLine("---");
            sb.AppendLine("xychart-beta");
            sb.AppendLine($"    title \"{title}\"");
            sb.AppendLine($"    x-axis [{labels}]");
            sb.AppendLine($"    y-axis \"{yAxis}\"");
            sb.AppendLine($"    bar [{values}]");
            sb.AppendLine("```").AppendLine();
        }

        // ---- JSON data for the interactive Chart.js page -----------------------------

        private static void WriteChartData(string docsDir, IReadOnlyList<Scenario> scenarios)
        {
            // Snapshotted libraries are absent from LibraryVersions (they did not run), but the
            // chart page derives its canonical library order — and therefore its colour
            // assignment and legend — from this dictionary, so they have to appear here too.
            var versions = LibraryVersions.All.ToDictionary(x => x.Library, x => x.Version);
            var snapshots = new Dictionary<string, string>();
            foreach (var (key, entry) in SnapshotRows)
            {
                versions[key.Library] = entry.Version;
                snapshots[key.Library] = LibrarySnapshot.Describe(entry);
            }

            var payload = new
            {
                updated = DateTime.Now.ToString("u", CultureInfo.InvariantCulture),
                versions,
                // Library -> provenance, for the libraries whose points were carried over from a
                // previous run rather than measured in this one.
                snapshots,
                scenarios = scenarios.Select(s => new
                {
                    key = s.Key,
                    label = s.Label,
                    libraries = s.ByLibrary.Select(b => b.Library).ToArray(),
                    // Non-null when the point was replayed from snapshots/ rather than measured
                    // in this run; the value describes the version/job/date it came from.
                    snapshotOf = s.ByLibrary.Select(b => b.SnapshotOf).ToArray(),
                    timeMs = s.ByLibrary.Select(b => b.TimeMs.HasValue ? Math.Round(b.TimeMs.Value, 2) : (double?)null).ToArray(),
                    allocMb = s.ByLibrary.Select(b => b.AllocMb.HasValue ? Math.Round(b.AllocMb.Value, 2) : (double?)null).ToArray(),
                    // Consumed by charts.html for the error whiskers and the tooltip's std-dev line.
                    errorMs = s.ByLibrary.Select(b => b.ErrorMs.HasValue ? Math.Round(b.ErrorMs.Value, 2) : (double?)null).ToArray(),
                    stdDevMs = s.ByLibrary.Select(b => b.StdDevMs.HasValue ? Math.Round(b.StdDevMs.Value, 3) : (double?)null).ToArray(),
                }).ToArray(),
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(docsDir, "results-data.js"), $"window.XLBENCH_DATA = {json};\n");
        }
    }
}
