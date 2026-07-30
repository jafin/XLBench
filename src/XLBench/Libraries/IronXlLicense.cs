namespace XLBench.Libraries;

/// <summary>
/// IronXL is the one commercial library in this suite, and on .NET it cannot be exercised at
/// all without a licence key.
/// </summary>
/// <remarks>
/// IronXL 2026.7.2 does not fall back to watermarked output when unlicensed — it throws
/// <c>IronSoftware.Exceptions.LicensingException("Production License Required")</c> from
/// <c>WorkBook.Create</c>/<c>Load</c>. The advertised "free for 7 days" development grace is
/// gated on an internal <c>DevelopmentEnvironmentDetected</c> check that reduces to:
/// <code>
/// Debugger.IsAttached || AppDomain.CurrentDomain.FriendlyName.EndsWith("vshost.exe", ...)
/// </code>
/// <c>vshost.exe</c> is the .NET Framework Visual Studio hosting process, which does not exist
/// on .NET Core or later — so on .NET 10 an attached debugger is the only qualifying signal.
/// BenchmarkDotNet measures in isolated child processes with no debugger, so the grace can
/// never apply to a benchmark run.
///
/// Supply a trial or full key in <c>XLBENCH_IRONXL_KEY</c> to include IronXL in the suite;
/// <see cref="XLBench.Config.BenchmarkConfig"/> filters its benchmarks out when the variable
/// is unset.
///
/// App analytics are switched off regardless. IronXL ships Grpc.Net.Client + Polly and phones
/// home on use; leaving that on would fold network latency and retry policy into the timings.
/// </remarks>
internal static class IronXlLicense
{
    private const string KeyVariable = "XLBENCH_IRONXL_KEY";

    private static readonly object Gate = new();
    private static bool _set;

    /// <summary>True when a key is configured — i.e. IronXL is worth attempting at all.</summary>
    public static bool KeyAvailable =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(KeyVariable));

    /// <summary>True when the configured key was accepted, so output is unwatermarked.</summary>
    public static bool IsLicensed { get; private set; }

    public static void Ensure()
    {
        if (_set) return;
        lock (Gate)
        {
            if (_set) return;

            IronXL.License.DisableAppAnalytics();

            var key = Environment.GetEnvironmentVariable(KeyVariable);
            if (!string.IsNullOrWhiteSpace(key))
                IronXL.License.LicenseKey = key;

            IsLicensed = IronXL.License.IsLicensed;
            _set = true;
        }
    }
}
