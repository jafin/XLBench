using System.Text;

namespace XLBench.Data;

/// <summary>
/// The single sheet layout every library's <c>CreateStockReport</c> benchmark must produce,
/// so the scenario is genuinely like-for-like and the saved artifacts are directly comparable
/// when opened side by side.
///
/// <code>
///        A       B              C        D      ...  V
///   1 | Week  | Week Ending  | AAPL   | ABBV  | ... | XOM      &lt;- header
///   2 | 1     | 2025-08-01   | 309.75 | ...            &lt;- week 1 (no prior week: not formatted)
///   3 | 2     | 2025-08-08   | 307.66 | ...            &lt;- week 2 onwards: conditionally formatted
///   ...
///  53 | 52    | 2026-07-24   | ...
/// </code>
///
/// Conditional formatting compares each price against the cell directly above it (the prior
/// week's close for the same symbol): green when it rose, red when it fell. Written as a
/// single pair of relative-reference expression rules over the whole block rather than one
/// rule per cell — that is the idiomatic Excel form, and it keeps the comparison about how
/// each library models conditional formatting rather than how fast it loops.
/// </summary>
public static class ReportLayout
{
    public const string SheetName = "Weekly Prices";
    public const string ChartTitle = "Weekly closing price by symbol";

    public const int HeaderRow = 1;
    public const int FirstDataRow = 2;

    public const int WeekNoCol = 1;
    public const int WeekEndingCol = 2;
    public const int FirstPriceCol = 3;

    public const string DateFormat = "yyyy-mm-dd";
    public const string PriceFormat = "#,##0.00";

    public static int LastPriceCol => FirstPriceCol + StockData.SymbolCount - 1;
    public static int LastDataRow => FirstDataRow + StockData.WeekCount - 1;

    /// <summary>
    /// First row the conditional format applies to. Week 1 is excluded — it has no prior
    /// week to compare against, and formatting it against the header row would be wrong.
    /// </summary>
    public static int CfFirstRow => FirstDataRow + 1;

    /// <summary>Range the conditional format covers, e.g. <c>C3:V53</c>.</summary>
    public static string CfRangeA1 =>
        $"{ColumnLetter(FirstPriceCol)}{CfFirstRow}:{ColumnLetter(LastPriceCol)}{LastDataRow}";

    /// <summary>
    /// Rule expression for "closed above the prior week". Relative references are resolved by
    /// Excel against the top-left cell of the applied range, so one expression covers the block.
    /// </summary>
    public static string UpFormula =>
        $"{ColumnLetter(FirstPriceCol)}{CfFirstRow}>{ColumnLetter(FirstPriceCol)}{CfFirstRow - 1}";

    /// <summary>Rule expression for "closed below the prior week".</summary>
    public static string DownFormula =>
        $"{ColumnLetter(FirstPriceCol)}{CfFirstRow}<{ColumnLetter(FirstPriceCol)}{CfFirstRow - 1}";

    // Excel's built-in "Good"/"Bad" cell-style colours, so the output looks native.
    public static readonly (byte R, byte G, byte B) UpFill = (0xC6, 0xEF, 0xCE);
    public static readonly (byte R, byte G, byte B) UpFont = (0x00, 0x61, 0x00);
    public static readonly (byte R, byte G, byte B) DownFill = (0xFF, 0xC7, 0xCE);
    public static readonly (byte R, byte G, byte B) DownFont = (0x9C, 0x00, 0x06);

    public static string Hex((byte R, byte G, byte B) c) => $"{c.R:X2}{c.G:X2}{c.B:X2}";

    /// <summary>Chart anchor: placed clear of the data block so it never overlaps.</summary>
    public static int ChartFirstCol => LastPriceCol + 1;
    public const int ChartFirstRow = 1;
    public const int ChartColSpan = 14;
    public const int ChartRowSpan = 30;

    /// <summary>Converts a 1-based column index to its A1 column letters (1 -> A, 27 -> AA).</summary>
    public static string ColumnLetter(int column)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(column, 1);

        var sb = new StringBuilder(3);
        while (column > 0)
        {
            var rem = (column - 1) % 26;
            sb.Insert(0, (char)('A' + rem));
            column = (column - 1) / 26;
        }
        return sb.ToString();
    }

    /// <summary>Absolute sheet-qualified reference for a price column's values, e.g. <c>'Weekly Prices'!$C$2:$C$53</c>.</summary>
    public static string PriceColumnRef(int symbolIndex)
    {
        var col = ColumnLetter(FirstPriceCol + symbolIndex);
        return $"'{SheetName}'!${col}${FirstDataRow}:${col}${LastDataRow}";
    }

    /// <summary>Absolute sheet-qualified reference for the shared category (week-ending) column.</summary>
    public static string CategoryRef()
    {
        var col = ColumnLetter(WeekEndingCol);
        return $"'{SheetName}'!${col}${FirstDataRow}:${col}${LastDataRow}";
    }
}
