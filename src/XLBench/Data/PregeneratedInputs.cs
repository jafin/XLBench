using System.Globalization;

namespace XLBench.Data;

/// <summary>
/// The input workbooks the scenarios open, handed from the main project to <c>XLBench.IronXL</c>
/// through <c>inputs/</c>.
/// </summary>
/// <remarks>
/// The scenario inputs are generated with ClosedXML as a neutral OOXML producer (see
/// <see cref="TestData"/>). IronXL cannot reference ClosedXML — their SixLabors.Fonts version
/// ranges do not overlap, which fails NuGet restore with NU1107 — so the IronXL project cannot
/// build them itself. Instead the main project writes them once (<c>dotnet run --project
/// src/XLBench -- inputs</c>) and the IronXL project loads the same bytes. Both libraries
/// therefore still open identical files.
/// </remarks>
internal static class PregeneratedInputs
{
    /// <summary>Repo-root <c>inputs/</c>, overridable with <c>XLBENCH_INPUTS</c>.</summary>
    public static string Directory =>
        Environment.GetEnvironmentVariable("XLBENCH_INPUTS") is { Length: > 0 } configured
            ? configured
            : Path.Combine(LibrarySnapshot.RepoRootOrCurrent(), "inputs");

    private static string PathFor(string name, string extension = "xlsx") =>
        Path.Combine(Directory, $"{name}.{extension}");

    /// <summary>Reads a workbook written by <c>WriteAll</c>.</summary>
    public static byte[] Load(string name)
    {
        var path = PathFor(name);
        return File.Exists(path)
            ? File.ReadAllBytes(path)
            : throw new FileNotFoundException(
                $"Input workbook '{path}' is missing. Generate it with: " +
                "dotnet run -c Release --project src/XLBench -- inputs", path);
    }

    /// <summary>Reads the checksum stored next to a workbook.</summary>
    public static long LoadChecksum(string name) =>
        long.Parse(File.ReadAllText(PathFor(name, "checksum")).Trim(), CultureInfo.InvariantCulture);

#if !XLBENCH_IRONXL
    /// <summary>Generates every scenario input and writes it to <see cref="Directory"/>.</summary>
    public static void WriteAll()
    {
        System.IO.Directory.CreateDirectory(Directory);

        Write("read", TestData.ReadXlsx);
        Write("edit", EditData.EditXlsx);
        Write("calc", CalcData.SourceXlsx);
        Write("properties", PropertiesData.SourceXlsx);
        Write("sheets", SheetsData.SourceXlsx);
        File.WriteAllText(PathFor("sheets", "checksum"),
            SheetsData.ExpectedChecksum.ToString(CultureInfo.InvariantCulture));

        Console.WriteLine($"[XLBench] Wrote scenario inputs to {Directory}");

        static void Write(string name, byte[] bytes)
        {
            File.WriteAllBytes(PathFor(name), bytes);
            Console.WriteLine($"[XLBench]   {name}.xlsx  {bytes.Length:N0} bytes");
        }
    }
#endif
}
