using BenchmarkDotNet.Attributes;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using XLBench.Data;

namespace XLBench.Benchmarks.Read;

/// <summary>
/// OpenXML SDK is a low-level streaming API — there is no eager "load the whole workbook
/// into an object model" operation, so only <see cref="OpenAndReadAll"/> is benchmarked.
/// Values are read via a SAX-style <see cref="OpenXmlReader"/> with the shared string table
/// materialized once (the idiomatic performant pattern).
/// </summary>
public class OpenXmlReadBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup() => _bytes = TestData.ReadXlsx;

    [Benchmark]
    public long OpenAndReadAll()
    {
        using var doc = SpreadsheetDocument.Open(new MemoryStream(_bytes), false);
        var wbPart = doc.WorkbookPart!;

        var shared = wbPart.SharedStringTablePart?.SharedStringTable is { } sst
            ? sst.Elements<SharedStringItem>().Select(si => si.InnerText).ToArray()
            : [];

        var wsPart = wbPart.WorksheetParts.First();

        long checksum = 0;
        using var reader = OpenXmlReader.Create(wsPart);
        while (reader.Read())
        {
            if (reader.ElementType != typeof(Cell)) continue;
            var cell = (Cell)reader.LoadCurrentElement()!;
            checksum += ResolveValue(cell, shared).Length;
        }
        return checksum;
    }

    private static string ResolveValue(Cell cell, string[] shared)
    {
        var raw = cell.CellValue?.InnerText ?? cell.InnerText;
        if (cell.DataType?.Value == CellValues.SharedString
            && int.TryParse(raw, out var idx)
            && idx >= 0 && idx < shared.Length)
        {
            return shared[idx];
        }
        return raw ?? string.Empty;
    }
}
