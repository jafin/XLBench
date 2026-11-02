using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Filters;
using XLBench.Libraries;

namespace XLBench.Config;

/// <summary>
/// Builds the single global config applied to every benchmark in this project. It starts from
/// <see cref="BenchmarkConfigCore"/> (joined summary, "Library" column, memory diagnoser,
/// GitHub-flavored markdown export), then drops libraries that cannot run without a licence.
/// IronXL is not here: it lives in <c>XLBench.IronXL</c> and reaches the published tables
/// through <c>snapshots/ironxl.json</c>.
/// </summary>
public static class BenchmarkConfig
{
    public static IConfig Create()
    {
        var config = BenchmarkConfigCore.Create();

        // RadSpreadProcessing degrades instead of throwing: unlicensed, every workbook it writes
        // gains an extra "License" worksheet. That is different work from what the other
        // libraries do and it would go unnoticed in the table, so drop it — see TelerikLicense.
        if (!TelerikLicense.IsLicensed)
        {
            Console.WriteLine(
                "[XLBench] No Telerik licence found — excluding Telerik benchmarks (see README).");
            config.AddFilter(new SimpleFilter(c =>
                !c.Descriptor.Type.Name.StartsWith("Telerik", StringComparison.Ordinal)));
        }

        return config;
    }
}
