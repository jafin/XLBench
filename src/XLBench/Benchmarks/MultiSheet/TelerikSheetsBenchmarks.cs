using BenchmarkDotNet.Attributes;
using Telerik.Documents.Spreadsheet.FormatProviders.OpenXml.Xlsx;
using Telerik.Documents.Spreadsheet.Model;
using XLBench.Data;
using XLBench.Libraries;

namespace XLBench.Benchmarks.MultiSheet;

/// <summary>
/// RadSpreadProcessing multi-sheet read: every sheet through <c>Workbook.Worksheets</c>, each
/// walked over its used range by index — there is no populated-cell enumeration, as noted in
/// <see cref="Read.TelerikReadBenchmarks"/>. Empty cells inside the range are skipped.
///
/// <para>The cell model has no rich-text value type, so a rich-text string arrives as a plain
/// <c>TextCellValue</c> holding the runs joined together.</para>
/// </summary>
public class TelerikSheetsBenchmarks
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
        using var workbook = new XlsxFormatProvider().Import(new MemoryStream(bytes), null).WithoutHistory();

        long cells = 0;
        long textChars = 0;
        foreach (var worksheet in workbook.Worksheets)
        {
            var used = worksheet.UsedCellRange;
            for (var row = used.FromIndex.RowIndex; row <= used.ToIndex.RowIndex; row++)
            for (var col = used.FromIndex.ColumnIndex; col <= used.ToIndex.ColumnIndex; col++)
            {
                var value = worksheet.Cells[row, col].GetValue().Value;
                if (value.ValueType == CellValueType.Empty) continue;

                cells++;
                if (value is TextCellValue text)
                    textChars += text.Value.Length;
            }
        }
        return SheetsData.Checksum(cells, textChars);
    }
}
