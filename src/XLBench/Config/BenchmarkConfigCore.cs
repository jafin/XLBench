using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace XLBench.Config;

/// <summary>
/// The part of the benchmark config shared by every project that runs benchmarks (the main
/// project and <c>XLBench.IronXL</c>), so both render identical tables.
/// </summary>
public static class BenchmarkConfigCore
{
    public static ManualConfig Create()
    {
        var config = ManualConfig.Create(DefaultConfig.Instance)
            .WithOption(ConfigOptions.JoinSummary, true)
            .AddColumn(new LibraryNameColumn())
            .AddDiagnoser(MemoryDiagnoser.Default)
            .AddExporter(MarkdownExporter.GitHub)
            // Pin the table units. BenchmarkDotNet otherwise picks one unit for the whole joined
            // table from the fastest row, so a single sub-millisecond result switches every row to
            // μs and KB. That also makes snapshotted rows, recorded in ms, unusable in the table.
            .WithSummaryStyle(SummaryStyle.Default
                .WithTimeUnit(Perfolizer.Horology.TimeUnit.Millisecond)
                .WithSizeUnit(Perfolizer.Metrology.SizeUnit.MB));

        // The Namespace column is always "XLBench.Benchmarks.Read/Write" — redundant, and it
        // widens the results table. Hide it (the Library + Type + Method columns identify each row).
        config.HideColumns("Namespace");
        return config;
    }
}

/// <summary>Maps a benchmark class name to the friendly library name shown in reports.</summary>
public static class LibraryNames
{
    public static string FromTypeName(string typeName) => typeName switch
    {
        var n when n.StartsWith("ClosedXml") => "ClosedXML",
        var n when n.StartsWith("EpPlus") => "EPPlus",
        var n when n.StartsWith("OpenXml") => "OpenXML SDK",
        var n when n.StartsWith("Npoi") => "NPOI",
        var n when n.StartsWith("MiniExcel") => "MiniExcel",
        var n when n.StartsWith("Sylvan") => "Sylvan",
        var n when n.StartsWith("XLibur") => "XLibur",
        var n when n.StartsWith("OfficeImo") => "OfficeIMO",
        var n when n.StartsWith("IronXl") => "IronXL",
        var n when n.StartsWith("Telerik") => "Telerik",
        _ => typeName,
    };
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

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase) =>
        LibraryNames.FromTypeName(benchmarkCase.Descriptor.Type.Name);

    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style) => GetValue(summary, benchmarkCase);
    public bool IsAvailable(Summary summary) => true;
    public bool AlwaysShow => true;
    public ColumnCategory Category => ColumnCategory.Job;
    public int PriorityInCategory => -10;
    public bool IsNumeric => false;
    public UnitType UnitType => UnitType.Dimensionless;
    public string Legend => "Excel library under test";
}
