namespace XLBench.Data;

/// <summary>
/// Artifact naming for the <c>CreateStockReport</c> scenario. See <see cref="ArtifactOutput"/>
/// for when and how the files are written.
/// </summary>
public static class ReportOutput
{
    /// <inheritdoc cref="ArtifactOutput.Directory"/>
    public static string Directory => ArtifactOutput.Directory;

    /// <summary>Builds the workbook once more, unmeasured, and writes it to <c>output/stock-report-{slug}.xlsx</c>.</summary>
    /// <param name="slug">Lower-case library identifier used in the file name, e.g. "epplus".</param>
    /// <param name="build">The same delegate the benchmark measures, targeting an arbitrary stream.</param>
    /// <returns><c>true</c> if the file was written; <c>false</c> if it was locked and skipped.</returns>
    public static bool Save(string slug, Action<Stream> build) =>
        ArtifactOutput.Save($"stock-report-{slug}.xlsx", build);
}
