using System.Text;

namespace XLBench.Data;

/// <summary>A1-reference helpers shared by the scenario layouts.</summary>
public static class A1
{
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
}
