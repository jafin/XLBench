using System.Runtime.CompilerServices;
using ClosedXML.Excel;

namespace XLBench.Data;

/// <summary>
/// The dataset behind the <c>OpenAmendPropertiesAndSave</c> scenario: open an existing workbook,
/// change two document properties, and write it back out.
///
/// <para><b>Init (not measured).</b> <see cref="SourceXlsx"/> is a single canonical .xlsx byte
/// buffer built once, lazily — a header row then <see cref="RowCount"/> data rows ×
/// <see cref="ColCount"/> numeric columns. Every library's benchmark opens these exact bytes.</para>
///
/// <code>
///        A          B          ...  H
///   1 | Column_1  | Column_2  | ... | Column_8      &lt;- header
///   2 | 5178.64   | 1420      | ... | 0.8317
///   3 | 9264.51   | 3907      | ... | 0.1122
///   ...
/// 1001 | ...
/// </code>
///
/// <para><b>Why this size.</b> Deliberately small. The scenario is a round trip whose interesting
/// part is the metadata edit and the serialization it forces, not throughput: at 8,000 cells the
/// grid is large enough that the file is a real workbook rather than a toy, and small enough that
/// it does not bury the round trip under the volume cost the <c>Read</c> and <c>Write</c>
/// scenarios already measure. The read scenario's 50,000 × 15 sheet would put every library
/// several hundred milliseconds into serialization and make the property edit invisible.</para>
///
/// <para>The file is generated with ClosedXML purely as a neutral producer of standard OOXML,
/// mirroring <see cref="TestData.ReadXlsx"/>; generation happens outside any measured region so
/// it does not bias results.</para>
/// </summary>
public static class PropertiesData
{
    /// <summary>Data rows in the generated workbook, below the header.</summary>
    public const int RowCount = 1_000;

    /// <summary>Numeric columns in the generated workbook.</summary>
    public const int ColCount = 8;

    public const string SheetName = "Numbers";
    public const int HeaderRow = 1;
    public const int FirstDataRow = HeaderRow + 1;

    /// <summary>Last sheet row holding data.</summary>
    public static int LastDataRow => FirstDataRow + RowCount - 1;

    /// <summary>The title the measured step writes into the workbook's document properties.</summary>
    public const string Title = "XLBench properties round trip";

    /// <summary>The category the measured step adds to the workbook's document properties.</summary>
    public const string Category = "Benchmark";

    private static readonly Lazy<byte[]> LazySourceXlsx = new(BuildSourceXlsx, isThreadSafe: true);

    /// <summary>Canonical .xlsx bytes every <c>OpenAmendPropertiesAndSave</c> benchmark opens.</summary>
    public static byte[] SourceXlsx => LazySourceXlsx.Value;

    /// <summary>
    /// Forces the workbook build to happen now, before any measurement. Every benchmark calls
    /// this from <c>[GlobalSetup]</c> for the reasons given on <see cref="StockData.EnsureLoaded"/>.
    /// </summary>
    public static void EnsureLoaded()
    {
        RuntimeHelpers.RunClassConstructor(typeof(PropertiesData).TypeHandle);
        _ = SourceXlsx;
    }

    private static byte[] BuildSourceXlsx()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.AddWorksheet(SheetName);

        for (var c = 1; c <= ColCount; c++)
            ws.Cell(HeaderRow, c).Value = $"Column_{c}";

#pragma warning disable S2245 // Deterministic seed is intentional for reproducible benchmarks
        var random = new Random(42);
#pragma warning restore S2245

        for (var i = 0; i < RowCount; i++)
        {
            var row = FirstDataRow + i;

            // A mix of magnitudes and precisions so the numbers serialize to varying widths
            // rather than to one repeated shape.
            ws.Cell(row, 1).Value = Math.Round(random.NextDouble() * 10000, 2);
            ws.Cell(row, 2).Value = random.Next(1, 5000);
            ws.Cell(row, 3).Value = Math.Round(random.NextDouble(), 4);
            ws.Cell(row, 4).Value = Math.Round(random.NextDouble() * 1000, 2);
            ws.Cell(row, 5).Value = random.Next(0, 100);
            ws.Cell(row, 6).Value = Math.Round(random.NextDouble() * -5000, 3);
            ws.Cell(row, 7).Value = random.Next(-1000, 1000);
            ws.Cell(row, 8).Value = Math.Round(random.NextDouble() * 100, 4);
        }

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}
