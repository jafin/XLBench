using BenchmarkDotNet.Attributes;
using Sylvan.Data.Excel;
using XLBench.Data;

namespace XLBench.Benchmarks.MultiSheet;

/// <summary>
/// Sylvan multi-sheet read: one forward-only <see cref="System.Data.Common.DbDataReader"/> over
/// the whole workbook, where each sheet is a result set and <c>NextResult()</c> moves to the next.
/// <c>ExcelSchema.NoHeaders</c> returns the header rows as data, as in
/// <see cref="Read.SylvanReadBenchmarks"/>.
///
/// <para>Text cells are recognised by <c>GetExcelDataType(i) == ExcelDataType.String</c>, and
/// <c>GetString</c> returns a rich-text cell's plain text.</para>
/// </summary>
public class SylvanSheetsBenchmarks
{
    private static readonly ExcelDataReaderOptions Options = new() { Schema = ExcelSchema.NoHeaders };

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
        using var reader = ExcelDataReader.Create(new MemoryStream(bytes), ExcelWorkbookType.ExcelXml, Options);

        long cells = 0;
        long textChars = 0;
        do
        {
            while (reader.Read())
            {
                for (var i = 0; i < reader.RowFieldCount; i++)
                {
                    var type = reader.GetExcelDataType(i);
                    if (type == ExcelDataType.Null) continue;

                    cells++;
                    if (type == ExcelDataType.String)
                        textChars += reader.GetString(i).Length;
                }
            }
        } while (reader.NextResult());
        return SheetsData.Checksum(cells, textChars);
    }
}
