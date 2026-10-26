namespace XLBench.Data;

/// <summary>
/// Artifact naming for the <c>FillTemplateAndSave</c> scenario. See <see cref="ArtifactOutput"/>
/// for when and how the files are written.
///
/// <para>Unlike the edit and insert scenarios, saving <i>is</i> the measured work here, so the
/// artifact is exactly what the benchmark produced, written to a file instead of a
/// <see cref="MemoryStream"/>. <see cref="TemplateArtifacts"/> re-opens each one and reports what
/// of the template survived.</para>
/// </summary>
public static class TemplateOutput
{
    /// <inheritdoc cref="ArtifactOutput.Directory"/>
    public static string Directory => ArtifactOutput.Directory;

    /// <summary>File name this library's artifact is written to, relative to <see cref="Directory"/>.</summary>
    /// <param name="slug">Lower-case library identifier, e.g. "epplus".</param>
    public static string FileName(string slug) => $"sales-template-{slug}.xlsx";

    /// <summary>Full path of this library's artifact.</summary>
    public static string PathFor(string slug) => Path.Combine(Directory, FileName(slug));

    /// <summary>Re-runs the fill, unmeasured, and saves it to <c>output/sales-template-{slug}.xlsx</c>.</summary>
    /// <param name="slug">Lower-case library identifier used in the file name, e.g. "epplus".</param>
    /// <param name="build">The same fill the benchmark measures, targeting an arbitrary stream.</param>
    /// <returns><c>true</c> if the file was written; <c>false</c> if it was locked and skipped.</returns>
    public static bool Save(string slug, Action<Stream> build) => ArtifactOutput.Save(FileName(slug), build);
}
