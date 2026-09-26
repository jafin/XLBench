using BenchmarkDotNet.Attributes;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using XLBench.Data;

namespace XLBench.Benchmarks.Read;

/// <summary>
/// OpenXML SDK is a low-level package API with no workbook object model. In
/// <see cref="OpenAndReadAll"/> values are read via a SAX-style <see cref="OpenXmlReader"/> with
/// the shared string table materialized once (the idiomatic performant pattern).
/// </summary>
public class OpenXmlReadBenchmarks
{
    private byte[] _bytes = null!;
    private byte[] _propertiesBytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        _bytes = TestData.ReadXlsx;
        PropertiesData.EnsureLoaded();
        _propertiesBytes = PropertiesData.SourceXlsx;
    }

    /// <inheritdoc cref="ClosedXmlReadBenchmarks.OpenAmendPropertiesAndSave"/>
    /// <remarks>
    /// The SDK edits a package in place, so the input is first copied into an expandable stream
    /// and that stream becomes the output. Title and Category are set on
    /// <c>PackageProperties</c>, the OPC core-properties part the package already points at, and
    /// disposing the document writes the package back into the stream. The worksheet parts are
    /// never parsed, which is the SDK's advantage here: only the parts that change are touched.
    /// </remarks>
    [Benchmark]
    public long OpenAmendPropertiesAndSave()
    {
        var output = new MemoryStream();
        output.Write(_propertiesBytes);

        using (var doc = SpreadsheetDocument.Open(output, isEditable: true))
        {
            doc.PackageProperties.Title = PropertiesData.Title;
            doc.PackageProperties.Category = PropertiesData.Category;
        }

        return output.Length;
    }

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
