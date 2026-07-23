using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace XLBench.Config;

/// <summary>
/// Builds the single global config applied to every benchmark. It starts from
/// <see cref="DefaultConfig.Instance"/> (so the console logger, default columns, analysers
/// and validators are kept), then joins all per-library classes into one summary with a
/// "Library" column, enables the memory diagnoser, and adds GitHub-flavored markdown export
/// for GitHub Pages.
/// </summary>
public static class BenchmarkConfig
{
    public static IConfig Create() =>
        ManualConfig.Create(DefaultConfig.Instance)
            .WithOption(ConfigOptions.JoinSummary, true)
            .AddColumn(new LibraryNameColumn())
            .AddDiagnoser(MemoryDiagnoser.Default)
            .AddExporter(MarkdownExporter.GitHub);
}

/// <summary>
/// Derives a friendly library name from the benchmark class name so the joined summary
/// can be grouped/sorted by the library under test.
/// </summary>
public sealed class LibraryNameColumn : IColumn
{
    public string Id => "Library";
    public string ColumnName => "Library";
    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
    {
        var typeName = benchmarkCase.Descriptor.Type.Name;
        return typeName switch
        {
            var n when n.StartsWith("ClosedXml") => "ClosedXML",
            var n when n.StartsWith("EpPlus") => "EPPlus",
            var n when n.StartsWith("OpenXml") => "OpenXML SDK",
            var n when n.StartsWith("Npoi") => "NPOI",
            var n when n.StartsWith("MiniExcel") => "MiniExcel",
            var n when n.StartsWith("XLibur") => "XLibur",
            _ => typeName,
        };
    }

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);
    public bool IsAvailable(Summary summary) => true;
    public bool AlwaysShow => true;
    public ColumnCategory Category => ColumnCategory.Job;
    public int PriorityInCategory => -10;
    public bool IsNumeric => false;
    public UnitType UnitType => UnitType.Dimensionless;
    public string Legend => "Excel library under test";
}
