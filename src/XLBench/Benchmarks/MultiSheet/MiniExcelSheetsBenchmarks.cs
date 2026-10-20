using BenchmarkDotNet.Attributes;
using MiniExcelLibs;
using XLBench.Data;

namespace XLBench.Benchmarks.MultiSheet;

/// <summary>
/// MiniExcel multi-sheet read: a streaming reader that works one sheet at a time, so the sheets
/// are listed with <c>GetSheetNames()</c> and each one is queried by name, row by row. Each row
/// is an <see cref="IDictionary{TKey,TValue}"/> keyed by column.
///
/// <para>Values come back typed, so text is a <see cref="string"/> (a rich-text cell as its plain
/// text) and numbers and dates are not.</para>
///
/// <para>Each <c>Query</c> opens the package again. That is MiniExcel's API for a named sheet,
/// and it is part of what is measured.</para>
/// </summary>
public class MiniExcelSheetsBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        SheetsData.EnsureLoaded();
        _bytes = SheetsData.SourceXlsx;
    }

    [Benchmark]
    public long OpenAndReadAllSheets() => Read(_bytes);

    /// <inheritdoc cref="ClosedXmlSheetsBenchmarks.Read"/>
    internal static long Read(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);

        long cells = 0;
        long textChars = 0;
        foreach (var sheet in stream.GetSheetNames())
        foreach (IDictionary<string, object?> row in stream.Query(useHeaderRow: false, sheetName: sheet, excelType: ExcelType.XLSX))
        foreach (var value in row.Values)
        {
            if (value is null) continue;

            cells++;
            if (value is string text)
                textChars += text.Length;
        }
        return SheetsData.Checksum(cells, textChars);
    }
}
