using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace XLBench.Data;

/// <summary>
/// Persisted results for a library that cannot participate in every run.
/// </summary>
/// <remarks>
/// IronXL is commercial and throws without a licence key (see
/// <see cref="XLBench.Libraries.IronXlLicense"/>), so most runs cannot measure it. Rather than
/// dropping it from the comparison entirely, a run that <em>does</em> have a key writes its
/// IronXL results to <c>snapshots/ironxl.json</c>, and later keyless runs fold that snapshot
/// back into the published tables and charts, clearly marked as carried over.
///
/// Entries are merged per method, so a filtered run (<c>--filter *Read*</c>) refreshes only the
/// methods it measured instead of discarding the rest. Each entry carries its own provenance —
/// library version, host, job and capture time — because a snapshot is by definition numbers
/// from a different run, and possibly a different machine or package version.
/// </remarks>
public sealed class LibrarySnapshot
{
    /// <summary>Friendly library name, matching the Library column (e.g. "IronXL").</summary>
    [JsonPropertyName("library")]
    public string Library { get; set; } = string.Empty;

    /// <summary>Measurements keyed by benchmark method name (e.g. "CreateStockReport").</summary>
    [JsonPropertyName("methods")]
    public Dictionary<string, SnapshotEntry> Methods { get; set; } = [];

    public sealed class SnapshotEntry
    {
        [JsonPropertyName("version")] public string Version { get; set; } = string.Empty;
        [JsonPropertyName("capturedUtc")] public string CapturedUtc { get; set; } = string.Empty;
        [JsonPropertyName("host")] public string Host { get; set; } = string.Empty;
        [JsonPropertyName("job")] public string Job { get; set; } = string.Empty;

        [JsonPropertyName("timeMs")] public double? TimeMs { get; set; }
        [JsonPropertyName("allocMb")] public double? AllocMb { get; set; }
        [JsonPropertyName("errorMs")] public double? ErrorMs { get; set; }
        [JsonPropertyName("stdDevMs")] public double? StdDevMs { get; set; }

        /// <summary>
        /// The library's row exactly as BenchmarkDotNet rendered it, plus the header it was
        /// rendered under. Replayed verbatim into the detailed results table so the snapshot
        /// shows the same columns and units as the live rows — but only when the current run's
        /// header matches, since a column-set change would misalign the cells.
        /// </summary>
        [JsonPropertyName("tableHeader")] public string? TableHeader { get; set; }

        [JsonPropertyName("tableRow")] public string? TableRow { get; set; }
    }

    // ---- Persistence -------------------------------------------------------------

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Repo-root <c>snapshots/</c>, overridable with <c>XLBENCH_SNAPSHOTS</c> so BenchmarkDotNet's
    /// child processes and CI can be pointed somewhere explicit.
    /// </summary>
    public static string Directory =>
        Environment.GetEnvironmentVariable("XLBENCH_SNAPSHOTS") is { Length: > 0 } configured
            ? configured
            : Path.Combine(RepoRootOrCurrent(), "snapshots");

    public static string PathFor(string library) =>
        Path.Combine(Directory, $"{library.ToLowerInvariant()}.json");

    /// <summary>Loads the snapshot for a library, or <c>null</c> when there is none (or it is unreadable).</summary>
    public static LibrarySnapshot? Load(string library)
    {
        var path = PathFor(library);
        if (!File.Exists(path)) return null;

        try
        {
            return JsonSerializer.Deserialize<LibrarySnapshot>(File.ReadAllText(path), Options);
        }
        catch (Exception ex) when (ex is JsonException or IOException)
        {
            // A corrupt snapshot must not fail an otherwise-good run; publish without it.
            Console.WriteLine($"[XLBench] Ignoring unreadable snapshot {path}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Merges <paramref name="measured"/> into the stored snapshot and writes it back, replacing
    /// only the methods just measured.
    /// </summary>
    public static void Merge(string library, IReadOnlyDictionary<string, SnapshotEntry> measured)
    {
        if (measured.Count == 0) return;

        var snapshot = Load(library) ?? new LibrarySnapshot { Library = library };
        snapshot.Library = library;
        foreach (var (method, entry) in measured)
            snapshot.Methods[method] = entry;

        var path = PathFor(library);
        try
        {
            System.IO.Directory.CreateDirectory(Directory);
            File.WriteAllText(path, JsonSerializer.Serialize(snapshot, Options) + "\n");
            Console.WriteLine(
                $"[XLBench] Snapshotted {measured.Count} {library} result(s) to {path} " +
                $"({snapshot.Methods.Count} method(s) stored).");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Console.WriteLine($"[XLBench] Could not write snapshot {path}: {ex.Message}");
        }
    }

    /// <summary>Human-readable provenance for the results page, e.g. "2026.7.2, ShortRun, captured 2026-07-30".</summary>
    public static string Describe(SnapshotEntry entry)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(entry.Version)) parts.Add(entry.Version);
        if (!string.IsNullOrWhiteSpace(entry.Job)) parts.Add(entry.Job);
        if (DateTime.TryParse(entry.CapturedUtc, CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind, out var captured))
            parts.Add($"captured {captured:yyyy-MM-dd}");
        return string.Join(", ", parts);
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
