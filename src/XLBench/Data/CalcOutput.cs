namespace XLBench.Data;

/// <summary>
/// Artifact naming for the <c>ChangePricesAndRecalculate</c> scenario. See
/// <see cref="ArtifactOutput"/> for when and how the files are written, and
/// <see cref="EditOutput"/> for why saving sits outside the measured region.
///
/// <para>As with the insert scenario, the benchmark reads a single cell back, but what the
/// scenario promises is that <i>every</i> formula came out right. <see cref="CalcArtifacts"/>
/// re-opens each saved file and checks every order's result and every summary cell.</para>
/// </summary>
public static class CalcOutput
{
    /// <inheritdoc cref="ArtifactOutput.Directory"/>
    public static string Directory => ArtifactOutput.Directory;

    /// <summary>File name this library's artifact is written to, relative to <see cref="Directory"/>.</summary>
    /// <param name="slug">Lower-case library identifier, e.g. "epplus".</param>
    public static string FileName(string slug) => $"orders-recalculated-{slug}.xlsx";

    /// <summary>Full path of this library's artifact.</summary>
    public static string PathFor(string slug) => Path.Combine(Directory, FileName(slug));

    /// <summary>Re-runs the scenario, unmeasured, and saves the result to <c>output/orders-recalculated-{slug}.xlsx</c>.</summary>
    /// <param name="slug">Lower-case library identifier used in the file name, e.g. "epplus".</param>
    /// <param name="build">Re-runs the scenario and serializes the resulting workbook to the stream.</param>
    /// <returns><c>true</c> if the file was written; <c>false</c> if it was locked and skipped.</returns>
    public static bool Save(string slug, Action<Stream> build) => ArtifactOutput.Save(FileName(slug), build);
}

/// <summary>
/// Outcome of one unmeasured calc pass: whether the artifact landed, and the grand total the
/// library recalculated. <see cref="Total"/> is <c>NaN</c> when the pass never ran because the
/// target file was locked.
/// </summary>
/// <param name="Saved">False when the target file was locked and the pass was skipped.</param>
/// <param name="Total">The library's recalculated value for the summary's grand total.</param>
public readonly record struct CalcResult(bool Saved, double Total);
