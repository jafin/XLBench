namespace XLBench.Data;

/// <summary>
/// The sheet layout behind the <c>InsertColumnsAndRecalculate</c> scenario — the mirror image of
/// <see cref="EditData"/>'s delete: nothing is removed, two columns are pushed into the middle of
/// the data, and every row total has to widen to take them in.
///
/// <para><b>Same dataset.</b> The source workbook is <see cref="EditData.EditXlsx"/> byte for byte
/// — <see cref="EditData.MaxRows"/> rows × 20 numeric columns, a <c>SUM(A..T)</c> row total in
/// column U, every second sheet row bold — so the two scenarios are directly comparable and only
/// the operation differs.</para>
///
/// <code>
///  before        A          B          ...  T          U
///     1 | Column_01 | Column_02 | ... | Column_20 | Total
///     2 | 45387     | 40887.69  | ... | 8040.24   | =SUM(A2:T2)
///
///  after         A     B     C          D          ...  V          W
///     1 | Column_01 |     |     | Column_02 | ... | Column_20 | Total
///     2 | 45387     |  10 |  10 | 40887.69  | ... | 8040.24   | =SUM(A2:V2)
/// </code>
///
/// <para><b>Benchmark (measured).</b> Open those bytes, insert
/// <see cref="InsertColumnCount"/> columns before column <see cref="InsertAtColumn"/>, write
/// <see cref="InsertedValue"/> into both of them on every data row, then recalculate. No row is
/// deleted and no existing value is overwritten, so a correct result is the original row total
/// plus <see cref="InsertedPerRow"/>.</para>
///
/// <para>The columns go <i>inside</i> the totalled range rather than at its edge, and that is the
/// point: <c>SUM(A2:T2)</c> has to come back as <c>SUM(A2:V2)</c> and pick the new columns up.
/// Inserting at column A would shift the range instead of widening it, which tests nothing. Where
/// the delete scenario makes each library re-point every formula 166 times, this one asks for a
/// single structural edit whose reference fixup runs across the whole workbook exactly once — so
/// it separates the per-operation overhead from the per-formula overhead the delete measures.</para>
/// </summary>
public static class InsertData
{
    /// <summary>1-based column the new columns are inserted before — B, i.e. inside <c>SUM(A:T)</c>.</summary>
    public const int InsertAtColumn = 2;

    /// <summary>How many columns the measured step inserts.</summary>
    public const int InsertColumnCount = 2;

    /// <summary>The value written into every inserted cell on every data row.</summary>
    public const double InsertedValue = 10d;

    /// <summary>What the inserted columns add to each row's total.</summary>
    public const double InsertedPerRow = InsertColumnCount * InsertedValue;

    /// <summary>Canonical .xlsx bytes every benchmark opens — the edit scenario's workbook, unchanged.</summary>
    public static byte[] SourceXlsx => EditData.EditXlsx;

    /// <inheritdoc cref="EditData.EnsureLoaded"/>
    public static void EnsureLoaded() => EditData.EnsureLoaded();

    public static int HeaderRow => EditData.HeaderRow;
    public static int FirstDataRow => EditData.FirstDataRow;

    /// <summary>Last data row. No row is deleted, so the sheet keeps its original height.</summary>
    public static int LastDataRow => EditData.LastDataRow;

    public static int RowCount => EditData.RowCount;

    /// <summary>1-based column index one past the last inserted column.</summary>
    public static int AfterInsertedColumn => InsertAtColumn + InsertColumnCount;

    /// <summary>
    /// 1-based index of the last populated column <i>before</i> the insert — the original <c>SUM</c>
    /// column, U. Libraries whose insert is expressed as a shift need the range to move.
    /// </summary>
    public static int LastColumnBeforeInsert => EditData.SumCol;

    /// <summary>1-based index of the last data column once the insert has pushed it right — V.</summary>
    public static int LastDataCol => EditData.ColCount + InsertColumnCount;

    /// <summary>1-based index of the <c>SUM</c> column once the insert has pushed it right — W.</summary>
    public static int SumCol => EditData.SumCol + InsertColumnCount;

    public static string SumColLetter => A1.ColumnLetter(SumCol);
    public static string LastDataColLetter => A1.ColumnLetter(LastDataCol);

    /// <summary>The widened row total formula for a sheet row, e.g. <c>SUM(A2:V2)</c>.</summary>
    public static string SumFormula(int sheetRow) => $"SUM(A{sheetRow}:{LastDataColLetter}{sheetRow})";

    /// <summary>
    /// The total a correct implementation must produce for a data row: every original value
    /// untouched, plus the two inserted <see cref="InsertedValue"/> cells.
    /// </summary>
    /// <param name="rowIndex">0-based CSV row index, which is also the row's position in the sheet.</param>
    public static double ExpectedTotal(int rowIndex)
    {
        var source = EditData.Values[rowIndex];
        var total = InsertedPerRow;
        foreach (var value in source)
            total += value;
        return total;
    }

    /// <summary>Total the last data row must show — the cell every benchmark reads back.</summary>
    public static double ExpectedLastTotal => ExpectedTotal(RowCount - 1);
}
