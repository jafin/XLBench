using System.Globalization;
using BenchmarkDotNet.Attributes;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using XLBench.Data;

namespace XLBench.Benchmarks.Edit;

/// <summary>
/// OpenXML SDK edit-and-recalculate: the whole scenario by hand, because the SDK is a
/// document-object model over the package and nothing more.
///
/// There is no <c>DeleteRow</c>: rows are <c>Remove()</c>d from <c>SheetData</c> and every
/// surviving row below then has its <c>r</c> attribute and each of its cells' <c>CellReference</c>
/// renumbered here, plus its <c>SUM</c> range rewritten to the row's new number. Nothing shifts
/// on its own.
///
/// <para>There is also no calculation engine — the SDK cannot evaluate <c>SUM</c>, so the
/// benchmark adds the row up itself and writes the result into the formula cell's cached
/// <c>&lt;v&gt;</c>. The stale <c>calcChain</c> part is dropped and <c>fullCalcOnLoad</c> set, so
/// Excel rebuilds the chain and re-derives the same numbers on open. Read this row's timing
/// against that: it is the only library here doing a single ordered pass with no model to
/// maintain, and the only one whose "recalculation" is arithmetic the benchmark wrote.</para>
/// </summary>
public class OpenXmlEditBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        EditData.EnsureLoaded();
        _bytes = EditData.EditXlsx;
    }

    [Benchmark]
    public double EditAndRecalculate() => Edit(new MemoryStream(_bytes), output: null);

    [GlobalCleanup]
    public void SaveArtifact() => WriteArtifact();

    /// <inheritdoc cref="ClosedXmlEditBenchmarks.WriteArtifact"/>
    internal static EditResult WriteArtifact()
    {
        var total = double.NaN;
        var saved = EditOutput.Save("openxml",
            output => total = Edit(new MemoryStream(EditData.EditXlsx), output));
        return new EditResult(saved, total);
    }

    /// <inheritdoc cref="ClosedXmlEditBenchmarks"/>
    private static double Edit(Stream input, Stream? output)
    {
        // The SDK edits the package in place, so unlike the libraries that parse into a model of
        // their own it needs a stream it can grow. MemoryStream over a byte[] is fixed-length.
        using var package = new MemoryStream();
        input.CopyTo(package);

        double total;
        using (var doc = SpreadsheetDocument.Open(package, isEditable: true))
        {
            var wbPart = doc.WorkbookPart!;
            var workbook = wbPart.Workbook!;
            var worksheet = wbPart.WorksheetParts.First().Worksheet!;

            total = RewriteRows(worksheet.GetFirstChild<SheetData>()!);
            UpdateDimension(worksheet);

            // The calculation chain lists formula cells by the row numbers they had before the
            // delete, so it is now wrong. Excel rebuilds it from scratch when it is absent.
            if (wbPart.CalculationChainPart is { } calcChain)
                wbPart.DeletePart(calcChain);

            workbook.CalculationProperties ??= new CalculationProperties();
            workbook.CalculationProperties.FullCalculationOnLoad = true;

            workbook.Save();
            worksheet.Save();
        }

        if (output is not null)
        {
            package.Position = 0;
            package.CopyTo(output);
        }
        return total;
    }

    /// <summary>
    /// Single ordered pass over <c>sheetData</c>: drops every third data row, renumbers what
    /// survives, overwrites column A, and rewrites each row's <c>SUM</c> formula and cached total.
    /// Returns the total of the last surviving row.
    /// </summary>
    private static double RewriteRows(SheetData sheetData)
    {
        var colCount = EditData.ColCount;
        var sumCol = EditData.SumCol;
        var toDelete = EditData.RowsToDelete.ToHashSet();

        var lastTotal = 0d;

        // Pre-incremented below, so the first row kept lands on HeaderRow.
        var newRowIndex = EditData.HeaderRow - 1;

        // ToList(): rows are removed while walking, which would otherwise mutate the live
        // child collection being enumerated.
        foreach (var row in sheetData.Elements<Row>().ToList())
        {
            var oldRowIndex = (int)row.RowIndex!.Value;

            if (toDelete.Contains(oldRowIndex))
            {
                row.Remove();
                continue;
            }

            newRowIndex++;
            var shifted = newRowIndex != oldRowIndex;
            if (shifted) row.RowIndex = (uint)newRowIndex;

            if (oldRowIndex == EditData.HeaderRow)
            {
                // Header: nothing to total, and it never shifts — it is the first row.
                continue;
            }

            Cell? sumCell = null;
            var total = EditData.ColumnAValue;

            foreach (var cell in row.Elements<Cell>())
            {
                var col = ColumnIndex(cell.CellReference!.Value!);
                if (shifted)
                    cell.CellReference = $"{A1.ColumnLetter(col)}{newRowIndex}";

                if (col == 1)
                {
                    cell.DataType = CellValues.Number;
                    cell.CellValue = new CellValue(
                        EditData.ColumnAValue.ToString("R", CultureInfo.InvariantCulture));
                }
                else if (col <= colCount)
                {
                    total += Number(cell);
                }
                else if (col == sumCol)
                {
                    sumCell = cell;
                }
            }

            if (sumCell is not null)
            {
                sumCell.CellFormula = new CellFormula(EditData.SumFormula(newRowIndex));
                sumCell.CellValue = new CellValue(total.ToString("R", CultureInfo.InvariantCulture));
            }

            lastTotal = total;
        }

        return lastTotal;
    }

    private static void UpdateDimension(Worksheet worksheet)
    {
        if (worksheet.GetFirstChild<SheetDimension>() is { } dimension)
        {
            dimension.Reference =
                $"A{EditData.HeaderRow}:{EditData.SumColLetter}{EditData.LastRowAfterEdit}";
        }
    }

    private static double Number(Cell cell) =>
        cell.CellValue is { } value
        && double.TryParse(value.InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)
            ? d
            : 0d;

    /// <summary>1-based column index from an A1 cell reference, e.g. "AB12" -> 28.</summary>
    private static int ColumnIndex(string cellReference)
    {
        var col = 0;
        foreach (var ch in cellReference)
        {
            if (!char.IsAsciiLetterUpper(ch)) break;
            col = (col * 26) + (ch - 'A' + 1);
        }
        return col;
    }
}
