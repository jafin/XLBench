namespace XLBench.Data;

/// <summary>
/// Artifact naming for the <c>EditAndRecalculate</c> scenario. See <see cref="ArtifactOutput"/>
/// for when and how the files are written.
///
/// <para>Saving is deliberately outside the measured region: the scenario is "open, edit,
/// recalculate", and serializing the result back out is what the <c>CreateAndSave</c> write
/// benchmark already measures. The artifacts exist so the edited workbook — shifted rows,
/// rewritten <c>SUM</c> ranges, recalculated totals — can be opened and checked by eye.</para>
/// </summary>
public static class EditOutput
{
    /// <inheritdoc cref="ArtifactOutput.Directory"/>
    public static string Directory => ArtifactOutput.Directory;

    /// <summary>Re-runs the edit, unmeasured, and saves the result to <c>output/numbers-edited-{slug}.xlsx</c>.</summary>
    /// <param name="slug">Lower-case library identifier used in the file name, e.g. "epplus".</param>
    /// <param name="build">Re-runs the edit and serializes the edited workbook to the stream.</param>
    /// <returns><c>true</c> if the file was written; <c>false</c> if it was locked and skipped.</returns>
    public static bool Save(string slug, Action<Stream> build) =>
        ArtifactOutput.Save($"numbers-edited-{slug}.xlsx", build);
}

/// <summary>
/// Outcome of one unmeasured edit pass: whether the artifact landed, and the total the library
/// recalculated for the last surviving row. <see cref="Total"/> is <c>NaN</c> when the edit never
/// ran because the target file was locked.
/// </summary>
/// <param name="Saved">False when the target file was locked and the pass was skipped.</param>
/// <param name="Total">The library's recalculated value for the last surviving row's SUM cell.</param>
public readonly record struct EditResult(bool Saved, double Total);
