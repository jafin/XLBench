using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using ClosedXML.Excel;

namespace XLBench.Data;

/// <summary>
/// The dataset and sheet layout behind the <c>EditAndRecalculate</c> scenario: open an existing
/// workbook, delete rows out of the middle of it, overwrite a column, and get the row totals
/// right afterwards.
///
/// <para><b>Init (not measured).</b> <see cref="EditXlsx"/> is a single canonical .xlsx byte
/// buffer built once, lazily, from the embedded <c>Data/numbers.csv</c> — <see cref="MaxRows"/>
/// rows × 20 numeric columns. Each row carries a <c>SUM(A..T)</c> total in column U, and every
/// second sheet row is bold. Every library's benchmark opens these exact bytes, so the comparison
/// is apples-to-apples.</para>
///
/// <code>
///        A          B          ...  T          U
///   1 | Column_01 | Column_02 | ... | Column_20 | Total          &lt;- header
///   2 | 45387     | 40887.69  | ... | 8040.24   | =SUM(A2:T2)    &lt;- bold (every 2nd sheet row)
///   3 | -43355.80 | -8116     | ... | 23652     | =SUM(A3:T3)
///   4 | -893      | -13663.81 | ... | -13611.14 | =SUM(A4:T4)    &lt;- deleted by the benchmark
///   ...
///  501 | ...
/// </code>
///
/// <para><b>Benchmark (measured).</b> Open those bytes, delete every third data row (sheet rows
/// 4, 7, 10 … — 166 of the 500), set column A to <see cref="ColumnAValue"/> on every
/// surviving data row, then recalculate so column U reflects both edits. Deleting a row has to
/// shift the rows below it up <i>and</i> rewrite their <c>SUM</c> ranges, which is the part that
/// separates the libraries: an eager cell model pays for the shift, a hand-authored one pays for
/// doing the renumbering itself, and only some of them own a calculation engine at all.</para>
///
/// <para>The file is generated with ClosedXML purely as a neutral producer of standard OOXML,
/// mirroring <see cref="TestData.ReadXlsx"/>; generation happens outside any measured region so
/// it does not bias results.</para>
/// </summary>
public static class EditData
{
    private const string ResourceName = "XLBench.Data.numbers.csv";

    public const string SheetName = "Numbers";
    public const int HeaderRow = 1;
    public const int FirstDataRow = HeaderRow + 1;

    /// <summary>
    /// Data rows taken from the head of <c>numbers.csv</c>, which holds 4,000.
    /// </summary>
    /// <remarks>
    /// Deleting a row is quadratic in every library that maintains a cell model — each delete
    /// shifts every row beneath it and rewrites their formula ranges — so the scenario's cost
    /// grows with the square of this number, not linearly. At the full 4,000 rows NPOI alone
    /// takes ~178 s per operation (it has no delete-and-close-gap call, so each deletion is a
    /// <c>RemoveRow</c> plus a whole-sheet <c>ShiftRows</c>), which is roughly 80 minutes of
    /// measured run for this scenario. 500 rows keeps the quadratic shape and the ranking
    /// between libraries while bringing the whole scenario down to a couple of minutes.
    /// </remarks>
    public const int MaxRows = 500;

    /// <summary>Every n-th data row is deleted by the measured step.</summary>
    public const int DeleteEveryNth = 3;

    /// <summary>Every n-th sheet row is bold in the generated file (init step, not measured).</summary>
    public const int BoldEveryNth = 2;

    /// <summary>The value the measured step writes into column A of every surviving data row.</summary>
    public const double ColumnAValue = 20d;

    public const string TotalHeader = "Total";

    /// <summary>Column headers from the CSV, one per numeric column.</summary>
    public static readonly string[] Headers;

    /// <summary>CSV values indexed <c>[rowIndex][columnIndex]</c>, both 0-based.</summary>
    public static readonly double[][] Values;

    public static int RowCount => Values.Length;
    public static int ColCount => Headers.Length;

    /// <summary>1-based index of the <c>SUM</c> column, immediately right of the data.</summary>
    public static int SumCol => ColCount + 1;

    public static string SumColLetter => A1.ColumnLetter(SumCol);
    public static string LastDataColLetter => A1.ColumnLetter(ColCount);

    public static int LastDataRow => FirstDataRow + RowCount - 1;

    /// <summary>The row total formula for a given sheet row, e.g. <c>SUM(A2:T2)</c>.</summary>
    public static string SumFormula(int sheetRow) => $"SUM(A{sheetRow}:{LastDataColLetter}{sheetRow})";

    /// <summary>True when the sheet row is one of the bold ones the init step produced.</summary>
    public static bool IsBoldRow(int sheetRow) => sheetRow % BoldEveryNth == 0;

    /// <summary>
    /// Sheet rows the measured step deletes, ascending. Defined over the data rows — data row 3,
    /// 6, 9 … — so the header is never a candidate. Libraries that delete one row at a time walk
    /// this in reverse, since deleting from the top invalidates every later row number.
    /// </summary>
    public static readonly int[] RowsToDelete;

    /// <summary>Original 0-based CSV row indices that survive the delete, in sheet order.</summary>
    public static readonly int[] SurvivingSourceRows;

    public static int SurvivingRowCount => SurvivingSourceRows.Length;

    /// <summary>Last sheet row still holding data once the deletes have shifted everything up.</summary>
    public static int LastRowAfterEdit => FirstDataRow + SurvivingRowCount - 1;

    /// <summary>
    /// The row total a correct implementation must produce for a surviving row: column A
    /// overwritten with <see cref="ColumnAValue"/>, columns B..T untouched.
    /// </summary>
    /// <param name="survivingIndex">0-based position among the surviving rows.</param>
    public static double ExpectedTotal(int survivingIndex)
    {
        var source = Values[SurvivingSourceRows[survivingIndex]];
        var total = ColumnAValue;
        for (var c = 1; c < source.Length; c++)
            total += source[c];
        return total;
    }

    /// <summary>Total the last surviving row must show — the cell every benchmark reads back.</summary>
    public static double ExpectedLastTotal => ExpectedTotal(SurvivingRowCount - 1);

    private static readonly Lazy<byte[]> LazyEditXlsx = new(BuildEditXlsx, isThreadSafe: true);

    /// <summary>Canonical .xlsx bytes every <c>EditAndRecalculate</c> benchmark opens.</summary>
    public static byte[] EditXlsx => LazyEditXlsx.Value;

    /// <summary>
    /// Forces the CSV parse and the workbook build to happen now, before any measurement.
    /// Every edit benchmark calls this from <c>[GlobalSetup]</c> for the reasons given on
    /// <see cref="StockData.EnsureLoaded"/>.
    /// </summary>
    public static void EnsureLoaded()
    {
        RuntimeHelpers.RunClassConstructor(typeof(EditData).TypeHandle);
        _ = EditXlsx;
    }

    static EditData()
    {
        (Headers, Values) = LoadCsv();

        var toDelete = new List<int>();
        var surviving = new List<int>();
        for (var i = 0; i < Values.Length; i++)
        {
            // i is 0-based, so data row (i + 1) is the n-th row in 1-based terms.
            if ((i + 1) % DeleteEveryNth == 0)
                toDelete.Add(FirstDataRow + i);
            else
                surviving.Add(i);
        }

        RowsToDelete = [.. toDelete];
        SurvivingSourceRows = [.. surviving];
    }

    private static (string[] Headers, double[][] Values) LoadCsv()
    {
        using var stream = typeof(EditData).GetTypeInfo().Assembly
                               .GetManifestResourceStream(ResourceName)
                           ?? throw new InvalidOperationException(
                               $"Embedded resource '{ResourceName}' not found. Ensure Data/numbers.csv " +
                               "is included as an <EmbeddedResource> in XLBench.csproj.");
        using var reader = new StreamReader(stream);

        var headerLine = reader.ReadLine()
                         ?? throw new InvalidOperationException($"'{ResourceName}' is empty.");
        var headers = headerLine.Split(',');

        var rows = new List<double[]>(MaxRows);
        while (rows.Count < MaxRows && reader.ReadLine() is { } line)
        {
            if (line.Length == 0) continue;

            var fields = line.Split(',');
            if (fields.Length != headers.Length)
            {
                throw new InvalidOperationException(
                    $"'{ResourceName}' row {rows.Count + 2} has {fields.Length} fields, expected {headers.Length}.");
            }

            var values = new double[fields.Length];
            for (var c = 0; c < fields.Length; c++)
            {
                values[c] = double.Parse(fields[c], NumberStyles.Float, CultureInfo.InvariantCulture);
            }
            rows.Add(values);
        }

        return rows.Count > 0
            ? (headers, [.. rows])
            : throw new InvalidOperationException($"'{ResourceName}' contains no data rows.");
    }

    private static byte[] BuildEditXlsx()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.AddWorksheet(SheetName);

        for (var c = 0; c < ColCount; c++)
            ws.Cell(HeaderRow, c + 1).Value = Headers[c];
        ws.Cell(HeaderRow, SumCol).Value = TotalHeader;

        for (var i = 0; i < RowCount; i++)
        {
            var row = FirstDataRow + i;
            var values = Values[i];
            for (var c = 0; c < values.Length; c++)
                ws.Cell(row, c + 1).Value = values[c];

            ws.Cell(row, SumCol).FormulaA1 = SumFormula(row);

            if (IsBoldRow(row))
                ws.Range(row, 1, row, SumCol).Style.Font.Bold = true;
        }

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}
