using BenchmarkDotNet.Attributes;
using OfficeIMO.Excel;
using XLBench.Data;

namespace XLBench.Benchmarks.MultiSheet;

/// <summary>
/// OfficeIMO multi-sheet read: every sheet through <c>ExcelDocument.Sheets</c>, every non-empty
/// cell through <c>ExcelSheet.EnumerateCells()</c>, as in <see cref="Read.OfficeImoReadBenchmarks"/>.
///
/// <para>Each cell arrives as an <c>ExcelCellValueInfo</c> whose <c>Value</c> is already typed, so
/// text is a <see cref="string"/> — for a rich-text cell, the runs joined into plain text.</para>
/// </summary>
public class OfficeImoSheetsBenchmarks
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
        using var doc = ExcelDocument.Load(new MemoryStream(bytes));

        long cells = 0;
        long textChars = 0;
        foreach (var ws in doc.Sheets)
        foreach (var cell in ws.EnumerateCells())
        {
            cells++;
            if (cell.Value is string text)
                textChars += text.Length;
        }
        return SheetsData.Checksum(cells, textChars);
    }
}
