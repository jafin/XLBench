using System.Globalization;
using BenchmarkDotNet.Attributes;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using XLBench.Data;

namespace XLBench.Benchmarks.Write;

/// <summary>
/// Writes via the low-level SAX <see cref="OpenXmlWriter"/> — the idiomatic performant path
/// for large sheets. Strings are written inline; the date column uses a cell style pointing
/// at the built-in date number format (numFmtId 14).
/// </summary>
public class OpenXmlWriteBenchmarks
{
    private const uint DateStyleIndex = 1;

    [Benchmark]
    public void CreateAndSave()
    {
        using var ms = new MemoryStream();
        using (var doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook))
        {
            var wbPart = doc.AddWorkbookPart();

            var stylesPart = wbPart.AddNewPart<WorkbookStylesPart>();
            stylesPart.Stylesheet = new Stylesheet(
                new Fonts(new Font()),
                new Fills(new Fill(new PatternFill { PatternType = PatternValues.None })),
                new Borders(new Border()),
                new CellFormats(
                    new CellFormat(),
                    new CellFormat { NumberFormatId = 14, ApplyNumberFormat = true }));
            stylesPart.Stylesheet.Save();

            var wsPart = wbPart.AddNewPart<WorksheetPart>();
            using (var writer = OpenXmlWriter.Create(wsPart))
            {
                writer.WriteStartElement(new Worksheet());
                writer.WriteStartElement(new SheetData());

                writer.WriteStartElement(new Row());
                WriteInlineString(writer, "Name");
                WriteInlineString(writer, "Amount");
                WriteInlineString(writer, "Date");
                writer.WriteEndElement();

                for (var i = 0; i < TestData.WriteRowCount; i++)
                {
                    writer.WriteStartElement(new Row());
                    WriteInlineString(writer, TestData.Strings[i]);
                    WriteNumber(writer, TestData.Numbers[i], null);
                    WriteNumber(writer, TestData.Dates[i].ToOADate(), DateStyleIndex);
                    writer.WriteEndElement();
                }

                writer.WriteStartElement(new Row());
                WriteInlineString(writer, "Total");
                WriteFormula(writer, $"SUM(B2:B{TestData.WriteRowCount + 1})");
                writer.WriteEndElement();

                writer.WriteEndElement(); // SheetData
                writer.WriteEndElement(); // Worksheet
            }

            wbPart.Workbook = new Workbook();
            var sheets = wbPart.Workbook.AppendChild(new Sheets());
            sheets.AppendChild(new Sheet
            {
                Id = wbPart.GetIdOfPart(wsPart),
                SheetId = 1,
                Name = "Data",
            });
            wbPart.Workbook.Save();
        }
    }

    private static void WriteInlineString(OpenXmlWriter w, string text)
    {
        w.WriteStartElement(new Cell { DataType = CellValues.InlineString });
        w.WriteStartElement(new InlineString());
        w.WriteElement(new Text(text));
        w.WriteEndElement(); // InlineString
        w.WriteEndElement(); // Cell
    }

    private static void WriteNumber(OpenXmlWriter w, double value, uint? styleIndex)
    {
        var cell = new Cell { DataType = CellValues.Number };
        if (styleIndex.HasValue) cell.StyleIndex = styleIndex.Value;
        w.WriteStartElement(cell);
        w.WriteElement(new CellValue(value.ToString(CultureInfo.InvariantCulture)));
        w.WriteEndElement();
    }

    private static void WriteFormula(OpenXmlWriter w, string formula)
    {
        w.WriteStartElement(new Cell());
        w.WriteElement(new CellFormula(formula));
        w.WriteEndElement();
    }
}
