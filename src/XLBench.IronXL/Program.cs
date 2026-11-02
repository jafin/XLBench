using System.Reflection;
using BenchmarkDotNet.Running;
using XLBench.Config;
using XLBench.Data;
using XLBench.Libraries;

// Measures IronXL and records the result in snapshots/ironxl.json, which the main XLBench run
// replays into the published tables. Run it before the main project:
//
//   dotnet run -c Release --project src/XLBench -- inputs          (once; writes inputs/)
//   dotnet run -c Release --project src/XLBench.IronXL -- --filter *Read*
//
// scripts/run-benchmarks.ps1 does both when XLBENCH_IRONXL_KEY is set.
//
// IronXL is commercial and throws without a licence key (see IronXlLicense), so without one there
// is nothing to measure and the existing snapshot is left alone.
if (!IronXlLicense.KeyAvailable)
{
    Console.WriteLine("[XLBench.IronXL] XLBENCH_IRONXL_KEY not set — nothing to measure; snapshots/ironxl.json unchanged.");
    return;
}

// `[GlobalCleanup]` saves review artifacts from BenchmarkDotNet's child process, whose working
// directory is the build folder. Pin the destination, as the main project does.
Environment.SetEnvironmentVariable("XLBENCH_OUTPUT", ReportOutput.Directory);

// Drop reports left by an earlier run so the snapshot is built only from this one.
var resultsDir = Path.Combine(Directory.GetCurrentDirectory(), "BenchmarkDotNet.Artifacts", "results");
if (Directory.Exists(resultsDir))
{
    foreach (var file in Directory.GetFiles(resultsDir, "*-report-github.md"))
    {
        try { File.Delete(file); } catch (IOException) { /* best effort */ }
    }
}

var summaries = BenchmarkSwitcher
    .FromAssembly(typeof(Program).Assembly)
    .Run(args, BenchmarkConfigCore.Create());

var version = VersionOf(typeof(IronXL.WorkBook));
var saved = SnapshotRecorder.Save("IronXL", version, summaries);
Console.WriteLine(saved > 0
    ? $"[XLBench.IronXL] Saved {saved} result(s) for IronXL {version} to {LibrarySnapshot.PathFor("IronXL")}. Commit it."
    : "[XLBench.IronXL] No results to save.");

static string VersionOf(Type type)
{
    var info = type.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    if (string.IsNullOrWhiteSpace(info)) return type.Assembly.GetName().Version?.ToString() ?? "unknown";
    var plus = info.IndexOf('+');
    return plus >= 0 ? info[..plus] : info;
}

