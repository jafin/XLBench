using OfficeOpenXml;

namespace XLBench.Libraries;

/// <summary>
/// EPPlus 8 requires a license to be declared before any <see cref="ExcelPackage"/> is used.
/// These benchmarks declare non-commercial use. Set the environment variable
/// <c>XLBENCH_EPPLUS_ORG</c> to override the organization name.
/// </summary>
internal static class EpPlusLicense
{
    private static readonly object Gate = new();
    private static bool _set;

    public static void Ensure()
    {
        if (_set) return;
        lock (Gate)
        {
            if (_set) return;
            var org = Environment.GetEnvironmentVariable("XLBENCH_EPPLUS_ORG") ?? "XLBench";
            ExcelPackage.License.SetNonCommercialOrganization(org);
            _set = true;
        }
    }
}
