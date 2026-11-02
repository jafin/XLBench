using System.Reflection;

namespace XLBench.Config;

/// <summary>
/// Resolves the actual package/assembly version of each Excel library under test by
/// reflecting a representative type from its assembly. Reported alongside the library name
/// so results state exactly which versions were benchmarked (auto-updates on package bumps).
/// </summary>
public static class LibraryVersions
{
    public static readonly IReadOnlyList<(string Library, string Version)> All = Build();

    private static IReadOnlyList<(string Library, string Version)> Build()
    {
        List<(string Library, string Version)> libraries =
        [
            ("ClosedXML", VersionOf(typeof(ClosedXML.Excel.XLWorkbook))),
        ("EPPlus", VersionOf(typeof(OfficeOpenXml.ExcelPackage))),
        ("OpenXML SDK", VersionOf(typeof(DocumentFormat.OpenXml.Packaging.SpreadsheetDocument))),
        ("NPOI", VersionOf(typeof(NPOI.XSSF.UserModel.XSSFWorkbook))),
        ("MiniExcel", VersionOf(typeof(MiniExcelLibs.MiniExcel))),
        ("Sylvan", VersionOf(typeof(Sylvan.Data.Excel.ExcelDataReader))),
            ("XLibur", VersionOf(typeof(XLibur.Excel.XLWorkbook))),
            ("OfficeIMO", VersionOf(typeof(OfficeIMO.Excel.ExcelDocument))),
        ];

        // Telerik, like IronXL, is only "under test" when it is licensed — unlicensed it
        // watermarks every export and its benchmarks are filtered out of the run.
        if (Libraries.TelerikLicense.IsLicensed)
            libraries.Add(("Telerik", VersionOf(typeof(Telerik.Documents.Spreadsheet.Model.Workbook))));

        return libraries;
    }

    /// <summary>
    /// Libraries classed as commercial. Emitted into <c>results-data.js</c> (as <c>commercial</c>) so the
    /// interactive charts can offer a one-click group toggle without hard-coding names in the page.
    /// </summary>
    public static readonly IReadOnlyList<string> Commercial = ["Telerik", "IronXL"];

    public static string For(string library) =>
        All.FirstOrDefault(x => x.Library == library).Version ?? string.Empty;

    private static string VersionOf(Type type)
    {
        var asm = type.Assembly;
        var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(info))
        {
            // Strip build metadata (e.g. "0.105.0+abc123") to keep the clean semver.
            var plus = info.IndexOf('+');
            return plus >= 0 ? info[..plus] : info;
        }
        return asm.GetName().Version?.ToString() ?? "unknown";
    }
}
