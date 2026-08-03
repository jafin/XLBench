namespace XLBench.Data;

/// <summary>
/// Artifact naming for the <c>InsertColumnsAndRecalculate</c> scenario. See
/// <see cref="ArtifactOutput"/> for when and how the files are written, and
/// <see cref="EditOutput"/> for why saving sits outside the measured region.
///
/// <para>The artifacts matter more here than in the edit scenario: the benchmark reads one cell
/// back, but what the scenario promises is that <i>every</i> row total came out right, and the
/// only place that can be checked is the saved file. <see cref="InsertArtifacts"/> re-opens each
/// one and verifies all of them.</para>
/// </summary>
public static class InsertOutput
{
    /// <inheritdoc cref="ArtifactOutput.Directory"/>
    public static string Directory => ArtifactOutput.Directory;

    /// <summary>File name this library's artifact is written to, relative to <see cref="Directory"/>.</summary>
    /// <param name="slug">Lower-case library identifier, e.g. "epplus".</param>
    public static string FileName(string slug) => $"numbers-inserted-{slug}.xlsx";

    /// <summary>Full path of this library's artifact.</summary>
    public static string PathFor(string slug) => Path.Combine(Directory, FileName(slug));

    /// <summary>Re-runs the insert, unmeasured, and saves the result to <c>output/numbers-inserted-{slug}.xlsx</c>.</summary>
    /// <param name="slug">Lower-case library identifier used in the file name, e.g. "epplus".</param>
    /// <param name="build">Re-runs the insert and serializes the resulting workbook to the stream.</param>
    /// <returns><c>true</c> if the file was written; <c>false</c> if it was locked and skipped.</returns>
    public static bool Save(string slug, Action<Stream> build) => ArtifactOutput.Save(FileName(slug), build);
}

/// <summary>
/// Outcome of one unmeasured insert pass: whether the artifact landed, and the total the library
/// recalculated for the last data row. <see cref="Total"/> is <c>NaN</c> when the insert never ran
/// because the target file was locked.
/// </summary>
/// <param name="Saved">False when the target file was locked and the pass was skipped.</param>
/// <param name="Total">The library's recalculated value for the last data row's SUM cell.</param>
public readonly record struct InsertResult(bool Saved, double Total);
