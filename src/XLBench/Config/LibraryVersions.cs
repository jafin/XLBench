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
            ("XLibur", VersionOf(typeof(XLibur.Excel.XLWorkbook))),
        ];

        // IronXL is referenced but only runs with a licence key; listing it as "under test"
        // when its benchmarks were filtered out would misreport the published results.
        if (Libraries.IronXlLicense.KeyAvailable)
            libraries.Add(("IronXL", VersionOf(typeof(IronXL.WorkBook))));

        return libraries;
    }

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
