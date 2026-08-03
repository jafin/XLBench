using System.Globalization;
using BenchmarkDotNet.Attributes;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using XLBench.Data;

namespace XLBench.Benchmarks.Insert;

/// <summary>
/// OpenXML SDK insert-columns-and-recalculate: the whole scenario by hand, because the SDK is a
/// document-object model over the package and nothing more.
///
/// There is no <c>InsertColumn</c>. Every cell from column B rightwards has its
/// <c>CellReference</c> rewritten two columns along, the two new cells are spliced into the gap in
/// column order — <c>SheetData</c> is an ordered element list, so appending them would produce a
/// file Excel rejects — and each row's <c>SUM</c> range is widened to the new last data column
/// here. Nothing shifts on its own.
///
/// <para>There is also no calculation engine — the SDK cannot evaluate <c>SUM</c>, so the
/// benchmark adds the row up itself and writes the result into the formula cell's cached
/// <c>&lt;v&gt;</c>. The <c>calcChain</c> part still lists the formula cells at their old column,
/// so it is dropped and <c>fullCalcOnLoad</c> set, letting Excel rebuild the chain and re-derive
/// the same numbers on open. Read this row's timing against that: it is the only library here
/// doing a single ordered pass with no model to maintain, and the only one whose "recalculation"
/// is arithmetic the benchmark wrote.</para>
/// </summary>
public class OpenXmlInsertBenchmarks
{
    private byte[] _bytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        InsertData.EnsureLoaded();
        _bytes = InsertData.SourceXlsx;
    }

    [Benchmark]
    public double InsertColumnsAndRecalculate() => Insert(new MemoryStream(_bytes), output: null);

    [GlobalCleanup]
    public void SaveArtifact() => WriteArtifact();

    /// <inheritdoc cref="ClosedXmlInsertBenchmarks.WriteArtifact"/>
    internal static InsertResult WriteArtifact()
    {
        var total = double.NaN;
        var saved = InsertOutput.Save("openxml",
            output => total = Insert(new MemoryStream(InsertData.SourceXlsx), output));
        return new InsertResult(saved, total);
    }

    /// <inheritdoc cref="ClosedXmlInsertBenchmarks"/>
    private static double Insert(Stream input, Stream? output)
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

            // The calculation chain lists each formula cell at the column it occupied before the
            // insert, so it is now wrong. Excel rebuilds it from scratch when it is absent.
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
    /// Single ordered pass over <c>sheetData</c>: shifts every cell from
    /// <see cref="InsertData.InsertAtColumn"/> rightwards, splices the inserted cells into the gap,
    /// and rewrites each row's <c>SUM</c> formula and cached total. Returns the total of the last
    /// data row.
    /// </summary>
    private static double RewriteRows(SheetData sheetData)
    {
        var shift = InsertData.InsertColumnCount;
        var sourceLastDataCol = EditData.ColCount;
        var sourceSumCol = InsertData.LastColumnBeforeInsert;

        var lastTotal = 0d;

        foreach (var row in sheetData.Elements<Row>())
        {
            var rowIndex = (int)row.RowIndex!.Value;

            // Every cell's original column, captured before any reference is rewritten.
            var cells = row.Elements<Cell>()
                .Select(c => (Cell: c, Column: ColumnIndex(c.CellReference!.Value!)))
                .ToList();

            Cell? sumCell = null;
            Cell? firstShifted = null;
            var total = InsertData.InsertedPerRow;

            foreach (var (cell, column) in cells)
            {
                if (column >= InsertData.InsertAtColumn)
                {
                    firstShifted ??= cell;
                    cell.CellReference = $"{A1.ColumnLetter(column + shift)}{rowIndex}";
                }

                if (column == sourceSumCol)
                    sumCell = cell;
                else if (column <= sourceLastDataCol)
                    total += Number(cell);
            }

            // The header keeps its (now shifted) labels; the inserted columns are left unlabelled,
            // so there is nothing more to do for it and no total to compute.
            if (rowIndex == InsertData.HeaderRow) continue;

            for (var col = InsertData.InsertAtColumn; col < InsertData.AfterInsertedColumn; col++)
            {
                var inserted = new Cell
                {
                    CellReference = $"{A1.ColumnLetter(col)}{rowIndex}",
                    DataType = CellValues.Number,
                    CellValue = new CellValue(
                        InsertData.InsertedValue.ToString("R", CultureInfo.InvariantCulture)),
                };

                // Cells must stay in column order. firstShifted is the cell that used to sit where
                // the new ones go, so each is spliced in ahead of it.
                if (firstShifted is not null) row.InsertBefore(inserted, firstShifted);
                else row.Append(inserted);
            }

            if (sumCell is not null)
            {
                sumCell.CellFormula = new CellFormula(InsertData.SumFormula(rowIndex));
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
                $"A{InsertData.HeaderRow}:{InsertData.SumColLetter}{InsertData.LastDataRow}";
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
