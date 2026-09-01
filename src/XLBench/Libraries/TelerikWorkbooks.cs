using Telerik.Documents.Spreadsheet.Model;

namespace XLBench.Libraries;

/// <summary>Shared setup for the RadSpreadProcessing benchmarks.</summary>
internal static class TelerikWorkbooks
{
    /// <summary>
    /// Turns off undo recording, which is on by default and is the single largest cost in the
    /// structural scenarios: every <c>Workbook</c> records each edit into
    /// <see cref="Telerik.Documents.Spreadsheet.History.WorkbookHistory"/> so a host application
    /// can undo it. Deleting 166 rows with history on takes ~110 s; with it off, ~34 s.
    /// </summary>
    /// <remarks>
    /// This is Telerik's own guidance for headless document processing — the model is shared with
    /// their editor controls, and nothing here is going to press Ctrl+Z. It is the same kind of
    /// opt-in as <c>evaluateFormulae</c> in the ClosedXML insert benchmark: a switch the library
    /// expects a non-interactive caller to throw, applied identically in every Telerik benchmark
    /// so the scenarios stay comparable with each other.
    ///
    /// <para>Suspending layout update (<c>Workbook.SuspendLayoutUpdate</c>) was measured
    /// alongside it and changed nothing, so it is not used.</para>
    /// </remarks>
    public static Workbook WithoutHistory(this Workbook workbook)
    {
        workbook.History.IsEnabled = false;
        return workbook;
    }
}
