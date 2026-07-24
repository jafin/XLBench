using ClosedXML.Excel;

namespace XLBench.Data;

/// <summary>
/// Shared, deterministic test data used by every library's benchmarks so the comparison
/// is apples-to-apples.
///
/// * <see cref="ReadXlsx"/> is a single canonical .xlsx byte buffer (built once, lazily)
///   that every read benchmark opens — so all libraries read the exact same bytes.
/// * <see cref="Strings"/>/<see cref="Numbers"/>/<see cref="Dates"/> are the in-memory
///   source columns every write benchmark serializes.
///
/// The read file is generated with ClosedXML purely as a neutral producer of standard
/// OOXML; generation happens outside any measured region so it does not bias results.
/// </summary>
public static class TestData
{
    public const int ReadRowCount = 200_000;
    public const int ReadColCount = 15;
    public const int WriteRowCount = 50_000;

    private static readonly Lazy<byte[]> LazyReadXlsx = new(BuildReadXlsx, isThreadSafe: true);

    /// <summary>Canonical .xlsx bytes shared by every read benchmark.</summary>
    public static byte[] ReadXlsx => LazyReadXlsx.Value;

    /// <summary>String column for write benchmarks (row 0 excluded — header is written by each benchmark).</summary>
    public static readonly string[] Strings = new string[WriteRowCount];
    public static readonly double[] Numbers = new double[WriteRowCount];
    public static readonly DateTime[] Dates = new DateTime[WriteRowCount];

    static TestData()
    {
#pragma warning disable S2245 // Deterministic seed is intentional for reproducible benchmarks
        var random = new Random(42);
#pragma warning restore S2245
        var baseDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        for (var i = 0; i < WriteRowCount; i++)
        {
            Strings[i] = $"Item {i} - {random.Next(1000):D4}";
            Numbers[i] = Math.Round(random.NextDouble() * 10000, 2);
            Dates[i] = baseDate.AddDays(random.Next(0, 1500));
        }
    }

    private static byte[] BuildReadXlsx()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.AddWorksheet("Data");

#pragma warning disable S2245 // Deterministic seed is intentional for reproducible benchmarks
        var random = new Random(42);
#pragma warning restore S2245
        var baseDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        string[] regions = { "North", "South", "East", "West", "Central" };
        string[] statuses = { "Active", "Pending", "Closed", "Review", "Draft" };

        for (var i = 0; i < ReadRowCount; i++)
        {
            var row = i + 1;
            var seed = random.Next(10000);

            // Cols 1-3: strings
            ws.Cell(row, 1).Value = $"Item {i}-{seed}";
            ws.Cell(row, 2).Value = $"Cat-{i % 12}";
            ws.Cell(row, 3).Value = regions[i % regions.Length];

            // Cols 4-8: numbers
            ws.Cell(row, 4).Value = Math.Round(random.NextDouble() * 10000, 2);
            ws.Cell(row, 5).Value = random.Next(1, 5000);
            ws.Cell(row, 6).Value = Math.Round(random.NextDouble(), 4);
            ws.Cell(row, 7).Value = Math.Round(random.NextDouble() * 1000, 2);
            ws.Cell(row, 8).Value = random.Next(0, 100);

            // Cols 9-11: dates
            ws.Cell(row, 9).Value = baseDate.AddDays(random.Next(0, 2000));
            ws.Cell(row, 10).Value = baseDate.AddDays(random.Next(0, 2000));
            ws.Cell(row, 11).Value = baseDate.AddDays(random.Next(0, 2000));

            // Cols 12-14: more strings
            ws.Cell(row, 12).Value = statuses[i % statuses.Length];
            ws.Cell(row, 13).Value = $"Note for row {row} with seed {seed}";
            ws.Cell(row, 14).Value = $"CODE-{seed:D5}";

            // Col 15: SUM formula referencing cols 4-8
            ws.Cell(row, 15).FormulaA1 = $"SUM(D{row}:H{row})";
        }

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}
