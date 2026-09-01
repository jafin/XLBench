using Telerik.Documents.Spreadsheet.FormatProviders.OpenXml.Xlsx;
using Telerik.Documents.Spreadsheet.Model;

namespace XLBench.Libraries;

/// <summary>
/// RadSpreadProcessing (Telerik Document Processing) is commercial, and unlike IronXL it does
/// not throw when unlicensed — it degrades. Every workbook it exports gains an extra worksheet
/// named <c>License</c> carrying a "License validation couldn't run" notice.
/// </summary>
/// <remarks>
/// That makes an unlicensed run quietly wrong rather than loudly broken: the write and report
/// benchmarks would serialize a sheet the other libraries do not, the saved artifacts would
/// carry a banner, and the numbers would still land in the published table looking ordinary.
/// So the benchmarks are gated the same way IronXL's are — see
/// <see cref="XLBench.Config.BenchmarkConfig"/>, which filters them out when
/// <see cref="IsLicensed"/> is false.
///
/// <para><b>Detection is a probe, not an API call.</b> Telerik resolves the licence at
/// <i>build</i> time: its MSBuild task finds a key (<c>TELERIK_LICENSE</c>,
/// <c>TELERIK_LICENSE_PATH</c>, a <c>telerik-license.txt</c> beside the project or in
/// <c>%AppData%\Telerik\</c>) and embeds it into the compiled assembly. There is no supported
/// runtime "am I licensed" call, so the honest check is to export a one-cell workbook and look
/// for the watermark sheet the unlicensed path adds. It costs a few milliseconds, once, outside
/// any measured region.</para>
///
/// <para>CI has no key, so Telerik is excluded there and its published numbers come from a
/// maintainer's licensed local run.</para>
/// </remarks>
internal static class TelerikLicense
{
    /// <summary>The worksheet the unlicensed export injects into every workbook.</summary>
    private const string WatermarkSheetName = "License";

    private static readonly Lazy<bool> LazyIsLicensed = new(Probe, isThreadSafe: true);

    /// <summary>True when exports come out clean — i.e. Telerik is worth benchmarking at all.</summary>
    public static bool IsLicensed => LazyIsLicensed.Value;

    private static bool Probe()
    {
        try
        {
            using var workbook = new Workbook();
            var worksheet = workbook.Worksheets.Add();
            worksheet.Cells[0, 0].SetValue(1d);

            using var stream = new MemoryStream();
            new XlsxFormatProvider().Export(workbook, stream, null);

            // Re-import rather than grep the bytes: the notice text is Telerik's to change, but
            // the extra sheet is what actually differs from a licensed export.
            stream.Position = 0;
            using var reimported = new XlsxFormatProvider().Import(stream, null);
            return !reimported.Worksheets.Any(sheet =>
                sheet.Name.Equals(WatermarkSheetName, StringComparison.OrdinalIgnoreCase));
        }
#pragma warning disable S2221 // Any failure to probe means "do not benchmark it", not "crash the run"
        catch (Exception ex)
#pragma warning restore S2221
        {
            Console.WriteLine($"[XLBench] Telerik licence probe failed, treating as unlicensed: {ex.Message}");
            return false;
        }
    }
}
