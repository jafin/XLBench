using System.Reflection;
using System.Runtime.CompilerServices;

namespace XLBench.Data;

/// <summary>
/// The template and dataset behind the <c>FillTemplateAndSave</c> scenario: open a workbook a
/// person built in Excel, fill its header cells and its table, and save it — the invoice / export
/// / monthly-report job, where the template carries the formatting and the code carries the data.
///
/// <para><b>Init (not measured).</b> <see cref="TemplateXlsx"/> is the embedded
/// <c>Data/sales_template.xlsx</c>, written by Excel itself (see <c>tools/TemplateGen</c>), not by
/// any library under test. That is the point of the scenario: every other scenario opens a file a
/// library produced, and a library reading its peers' output is an easier test than reading
/// Excel's — its theme, named cell styles, table part, chart with <c>c14</c>/<c>c16r2</c>
/// extensions and <c>x14ac</c> row attributes. <see cref="Rows"/> is a seeded, deterministic set of
/// <see cref="RowCount"/> sales lines.</para>
///
/// <code>
///        A             B          C          D          E           F          H          I
///   1 | Monthly Sales Report  (merged A1:F1, ReportTitle)        |          |          |
///   2 | Report date | 2026-01-31 (ReportDate)                    |          |          |
///   3 | Region      | All regions (Region)                       |          |          |
///   5 | Date        | Product  | Category | Quantity | UnitPrice | Amount    | Category | Amount
///   6 | (empty row — table "Sales" is A5:F6)                     |           | Hardware | =SUMIFS(Sales[Amount],Sales[Category],H6)
///  ...                                                                       | ...      |
///  11 |                                                                      | Total    | =SUBTOTAL(109,Sales[Amount])
///  13 |                                                                      | column chart of H5:I10
/// </code>
///
/// <para><b>Benchmark (measured).</b> Open those bytes, write <see cref="Title"/>,
/// <see cref="ReportDate"/> and <see cref="Region"/> into the three named cells, write all
/// <see cref="RowCount"/> rows into the table from <see cref="FirstDataRow"/> down (each
/// <c>Amount</c> as the A1 formula <see cref="AmountFormula"/>), grow the table to
/// <see cref="TableRef"/>, and save. Nothing is recalculated: the template's summary formulas use
/// structured references, which follow the table as it grows, and Excel recomputes them on
/// open.</para>
///
/// <para><b>Why the chart plots the summary block.</b> A chart over the table's own columns stores
/// a fixed A1 range, and no library here extends a chart range when it extends a table — Excel
/// only does that interactively. Plotting the fixed five-row summary block is how a template
/// author avoids that, and it reduces the chart to a preservation question: is it still there, and
/// does it still point at <c>H6:I10</c>.</para>
/// </summary>
public static class TemplateData
{
    private const string ResourceName = "XLBench.Data.sales_template.xlsx";

    public const string SheetName = "Report";
    public const string TableName = "Sales";

    /// <summary>Sales lines the measured step writes into the table.</summary>
    public const int RowCount = 2_000;

    /// <summary>Sheet row of the table's header. The template's one empty data row sits below it.</summary>
    public const int HeaderRow = 5;
    public const int FirstDataRow = HeaderRow + 1;
    public static int LastDataRow => FirstDataRow + RowCount - 1;

    /// <summary>Columns of the table, A..F.</summary>
    public const int ColumnCount = 6;
    public const int DateCol = 1;
    public const int ProductCol = 2;
    public const int CategoryCol = 3;
    public const int QuantityCol = 4;
    public const int UnitPriceCol = 5;
    public const int AmountCol = 6;

    /// <summary>The template's table range before the fill: header plus one empty row.</summary>
    public const string TemplateTableRef = "A5:F6";

    /// <summary>The table range a correct fill leaves behind, e.g. <c>A5:F2005</c>.</summary>
    public static string TableRef => $"A{HeaderRow}:{A1.ColumnLetter(ColumnCount)}{LastDataRow}";

    /// <summary>The named cells the template defines, and where they point.</summary>
    public const string TitleName = "ReportTitle";
    public const string DateName = "ReportDate";
    public const string RegionName = "Region";

    public const string TitleCell = "A1";
    public const string DateCell = "B2";
    public const string RegionCell = "B3";

    /// <summary>What the measured step writes into the named cells.</summary>
    public const string Title = "Sales Report — January 2026";
    public static readonly DateTime ReportDate = new(2026, 1, 31);
    public const string Region = "EMEA";

    /// <summary>The merged range the template's title occupies.</summary>
    public const string TitleMerge = "A1:F1";

    /// <summary>Where the template's validation and conditional formatting apply.</summary>
    public const string ValidationRange = "C6:C5000";
    public const string ConditionalFormatRange = "F6:F5000";

    /// <summary>The series ranges the template's chart plots — the fixed summary block.</summary>
    public const string ChartCategories = "Report!$H$6:$H$10";
    public const string ChartValues = "Report!$I$6:$I$10";

    /// <summary>The A1 formula each row's Amount cell carries, e.g. <c>D6*E6</c>.</summary>
    public static string AmountFormula(int sheetRow) => $"D{sheetRow}*E{sheetRow}";

    /// <summary>
    /// 1-based row and column of a single-cell reference, tolerating the sheet prefix and
    /// <c>$</c> anchors a defined name carries — <c>=Report!$A$1</c>, <c>Report!$A$1</c> and
    /// <c>A1</c> all give (1, 1). For the libraries whose name lookup hands back text rather than
    /// a cell.
    /// </summary>
    public static (int Row, int Column) ParseCell(string reference)
    {
        var text = reference.AsSpan().TrimStart('=');
        if (text.LastIndexOf('!') is var bang and >= 0)
            text = text[(bang + 1)..];

        var row = 0;
        var col = 0;
        foreach (var ch in text)
        {
            if (char.IsAsciiLetter(ch)) col = (col * 26) + (char.ToUpperInvariant(ch) - 'A' + 1);
            else if (char.IsAsciiDigit(ch)) row = (row * 10) + (ch - '0');
        }
        return row > 0 && col > 0
            ? (row, col)
            : throw new FormatException($"'{reference}' is not a single-cell reference.");
    }

    /// <summary>One sales line, as the table's columns hold it.</summary>
    public sealed record SalesRow(DateTime Date, string Product, string Category, int Quantity, double UnitPrice)
    {
        public double Amount => Quantity * UnitPrice;
    }

    /// <summary>Seeded so every library, every run, writes the same lines.</summary>
    public static readonly SalesRow[] Rows = BuildRows();

    /// <summary>The line the last data row must hold — what the artifact check reads back.</summary>
    public static SalesRow LastRow => Rows[^1];

    private static readonly Lazy<byte[]> LazyTemplateXlsx = new(LoadTemplate, isThreadSafe: true);

    /// <summary>The Excel-authored template bytes every <c>FillTemplateAndSave</c> benchmark opens.</summary>
    public static byte[] TemplateXlsx => LazyTemplateXlsx.Value;

    /// <summary>
    /// Forces the row generation and the resource read to happen now, before any measurement.
    /// Every template benchmark calls this from <c>[GlobalSetup]</c> for the reasons given on
    /// <see cref="StockData.EnsureLoaded"/>.
    /// </summary>
    public static void EnsureLoaded()
    {
        RuntimeHelpers.RunClassConstructor(typeof(TemplateData).TypeHandle);
        _ = TemplateXlsx;
    }

    private static SalesRow[] BuildRows()
    {
        (string Product, string Category, double Price)[] catalogue =
        [
            ("Laptop 14\"", "Hardware", 1249.00),
            ("Laptop 16\"", "Hardware", 1799.00),
            ("Monitor 27\"", "Hardware", 329.99),
            ("Docking station", "Hardware", 189.50),
            ("Office suite", "Software", 99.00),
            ("Antivirus", "Software", 39.95),
            ("Design suite", "Software", 649.00),
            ("Backup service", "Software", 7.50),
            ("Installation", "Services", 150.00),
            ("Support hour", "Services", 85.00),
            ("Data migration", "Services", 1200.00),
            ("Keyboard", "Accessories", 49.99),
            ("Mouse", "Accessories", 24.99),
            ("Headset", "Accessories", 79.00),
            ("USB-C cable", "Accessories", 12.49),
            ("Onboarding course", "Training", 299.00),
            ("Advanced workshop", "Training", 890.00),
            ("E-learning seat", "Training", 45.00),
        ];

        var random = new Random(20260131);
        var start = new DateTime(2026, 1, 1);
        var rows = new SalesRow[RowCount];
        for (var i = 0; i < rows.Length; i++)
        {
            var (product, category, price) = catalogue[random.Next(catalogue.Length)];
            rows[i] = new SalesRow(
                Date: start.AddDays(i * 31 / RowCount),
                Product: product,
                Category: category,
                Quantity: random.Next(1, 40),
                UnitPrice: price);
        }
        return rows;
    }

    private static byte[] LoadTemplate()
    {
        using var stream = typeof(TemplateData).GetTypeInfo().Assembly
                               .GetManifestResourceStream(ResourceName)
                           ?? throw new InvalidOperationException(
                               $"Embedded resource '{ResourceName}' not found. Ensure Data/sales_template.xlsx " +
                               "is included as an <EmbeddedResource> in XLBench.csproj.");
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}
