using System.Runtime.CompilerServices;
using ClosedXML.Excel;

namespace XLBench.Data;

/// <summary>
/// The workbook behind the <c>ChangePricesAndRecalculate</c> scenario: a small product price list,
/// a thousand order lines that look their prices up in it, and a summary sheet that aggregates
/// the orders — the shape of an ordinary sales workbook, and nearly all of it formulas.
///
/// <para><b>Init (not measured).</b> <see cref="SourceXlsx"/> is a single canonical .xlsx byte
/// buffer built once, lazily, from a seeded generator, so every run and every library sees the same
/// numbers. Three sheets:</para>
///
/// <code>
///  Products   A          B            C           D
///     1 | ProductId | Category   | UnitPrice | DiscountRate
///     2 | P001      | Hardware   | 412       | 0.10
///   ... 200 products
///
///  Orders     A        B           C          D        E          F          G        H          I
///     1 | OrderId | ProductId | Quantity | Region | UnitPrice | Category | Gross  | Discount | Net
///     2 | 1       | P117      | 64       | East   | =IFERROR(VLOOKUP(…))  | =IFERROR(INDEX(…MATCH(…)))
///                                                             | =C2*E2 | =IF(C2>=50,…) | =ROUND(G2-H2,2)
///   ... OrderCount orders; every 97th names a product that does not exist
///
///  Summary    A          B…E (one per region)                          F
///     1 | Category | North … West                                    | Orders
///     2 | Hardware | =SUMIFS(Orders!I, Orders!F, $A2, Orders!D, B$1) | =COUNTIFS(Orders!F, $A2)
///   ... one row per category, then "Unknown", a totals row, and three statistics
/// </code>
///
/// <para><b>Benchmark (measured).</b> Open those bytes, raise every product's price by
/// <see cref="PriceIncrease"/>, recalculate, and read back the grand total. Changing the price list
/// dirties every order line, so the whole dependency graph has to be re-evaluated: two
/// <c>VLOOKUP</c>s and an <c>INDEX</c>/<c>MATCH</c> per order over a 200-row table, the arithmetic
/// on top, and then 36 <c>SUMIFS</c> and 9 <c>COUNTIFS</c> that each scan every order. The grand
/// total depends on all of it, so an engine that evaluates lazily cannot skip any of the work.</para>
///
/// <para>The functions are the everyday core of a business workbook — lookups, error trapping,
/// conditional aggregates — rather than anything exotic. The dependency graph is wide but shallow
/// on purpose: no formula depends on the row above it, so an engine that evaluates recursively is
/// never asked for a call stack as deep as the sheet is long, which would test the stack size
/// rather than the engine. Ranges are bounded (<c>$I$2:$I$1001</c>), not whole columns, so the
/// work is the same whether or not an engine trims a whole-column reference to the used range.</para>
///
/// <para>Every 97th order names a product that is not in the list. Its <c>VLOOKUP</c> and
/// <c>MATCH</c> fail with <c>#N/A</c>, <c>IFERROR</c> turns that into <c>0</c> or
/// <c>"Unknown"</c>, and the summary gives those orders a row of their own. Trapping a failed lookup
/// is part of what an everyday engine has to get right, and it is where the engines here
/// differ.</para>
///
/// <para>Prices are whole dollars and discount rates are multiples of 5%, so every
/// <c>ROUND(…, 2)</c> argument has at most two decimals and no engine's midpoint rule can change
/// the answer. The expected values are computed directly in C# (<see cref="Expected"/>), never by
/// a library.</para>
///
/// <para><b>Who takes part.</b> Only libraries with a calculation engine of their own. The OpenXML
/// SDK, MiniExcel and Sylvan have none, and doing the arithmetic in the benchmark — as the edit
/// scenario does for the SDK — would measure C#, not an engine. RadSpreadProcessing has an engine
/// but no <c>IFERROR</c>, <c>SUMIFS</c> or <c>COUNTIFS</c> (none of the three is registered in its
/// <c>FunctionManager</c>; each evaluates to <c>#NAME?</c>), so it cannot compute this workbook
/// and is left out rather than timed producing errors.</para>
///
/// <para>The file is generated with ClosedXML purely as a neutral producer of standard OOXML,
/// mirroring <see cref="EditData"/>. ClosedXML writes no cached values, so every library starts
/// from formulas alone and has to evaluate all of them.</para>
/// </summary>
public static class CalcData
{
    public const string ProductsSheet = "Products";
    public const string OrdersSheet = "Orders";
    public const string SummarySheet = "Summary";

    public const int HeaderRow = 1;
    public const int FirstDataRow = HeaderRow + 1;

    /// <summary>Rows in the price list.</summary>
    public const int ProductCount = 200;

    /// <summary>Order lines, each one a row of five formulas.</summary>
    /// <remarks>
    /// ClosedXML, EPPlus, NPOI and XLibur each take about a second per operation at 1,000 orders
    /// and two to four seconds at 5,000. Every order still costs the same lookups and every summary
    /// cell still scans every order, so the size changes the totals, not the shape of the work.
    /// OfficeIMO, the engine that originally set this size, is excluded from the scenario — see
    /// the README's Calc capability matrix.
    /// </remarks>
    public const int OrderCount = 1_000;

    /// <summary>Every n-th order names <see cref="UnknownProductId"/>, so its lookups fail.</summary>
    public const int UnknownEveryNth = 97;

    /// <summary>A product id that is not in the price list.</summary>
    public const string UnknownProductId = "P999";

    /// <summary>What <c>IFERROR</c> turns a failed category lookup into.</summary>
    public const string UnknownCategory = "Unknown";

    /// <summary>Orders of at least this quantity get the product's discount.</summary>
    public const int DiscountQuantity = 50;

    /// <summary>What the measured step adds to every product's unit price.</summary>
    public const double PriceIncrease = 3d;

    /// <summary>Seed for the generator. Changing it changes every expected value.</summary>
    private const int Seed = 20261020;

    public static readonly string[] Categories =
        ["Hardware", "Software", "Services", "Networking", "Storage", "Peripherals", "Licensing", "Training"];

    public static readonly string[] Regions = ["North", "South", "East", "West"];

    private static readonly double[] DiscountRates = [0.05, 0.10, 0.15, 0.20];

    // ---- Products sheet columns (1-based) --------------------------------------------------

    public const int ProductIdCol = 1;
    public const int ProductCategoryCol = 2;
    public const int UnitPriceCol = 3;
    public const int DiscountRateCol = 4;

    public static int LastProductRow => FirstDataRow + ProductCount - 1;

    // ---- Orders sheet columns (1-based) ----------------------------------------------------

    public const int OrderIdCol = 1;
    public const int OrderProductCol = 2;
    public const int QuantityCol = 3;
    public const int RegionCol = 4;
    public const int OrderPriceCol = 5;
    public const int OrderCategoryCol = 6;
    public const int GrossCol = 7;
    public const int DiscountCol = 8;
    public const int NetCol = 9;

    public static int LastOrderRow => FirstDataRow + OrderCount - 1;

    // ---- Summary sheet layout (1-based) ----------------------------------------------------

    /// <summary>Every category plus <see cref="UnknownCategory"/>, one summary row each.</summary>
    public static readonly string[] SummaryCategories = [.. Categories, UnknownCategory];

    /// <summary>First region column of the summary grid (B); one column per region.</summary>
    public const int FirstRegionCol = 2;

    public static int LastRegionCol => FirstRegionCol + Regions.Length - 1;

    /// <summary>The per-category <c>COUNTIFS</c> column, right of the region columns (F).</summary>
    public static int CountCol => LastRegionCol + 1;

    public static int LastCategoryRow => FirstDataRow + SummaryCategories.Length - 1;

    /// <summary>Row of per-region totals, directly under the grid.</summary>
    public static int TotalsRow => LastCategoryRow + 1;

    /// <summary>Row holding <c>SUM</c> of the whole grid — the cell every benchmark reads back.</summary>
    public static int GrandTotalRow => TotalsRow + 2;

    public static int AverageNetRow => GrandTotalRow + 1;
    public static int MaxNetRow => GrandTotalRow + 2;

    /// <summary>Column the three statistics' values sit in (B), with their labels in A.</summary>
    public const int StatValueCol = 2;

    /// <summary>A1 address of the grand total, e.g. <c>B13</c>, on <see cref="SummarySheet"/>.</summary>
    public static string GrandTotalAddress => $"{A1.ColumnLetter(StatValueCol)}{GrandTotalRow}";

    // ---- Source data -----------------------------------------------------------------------

    /// <summary>One row of the price list, as generated.</summary>
    public readonly record struct Product(string Id, string Category, double UnitPrice, double DiscountRate);

    /// <summary>One order line's inputs, as generated. Its five formula columns are derived.</summary>
    public readonly record struct Order(int Id, string ProductId, int Quantity, string Region);

    public static readonly Product[] Products;
    public static readonly Order[] Orders;

    /// <summary>What a correct engine must produce once the prices have been raised.</summary>
    public static readonly CalcExpected Expected;

    private static readonly Lazy<byte[]> LazySourceXlsx = new(BuildSourceXlsx, isThreadSafe: true);

    /// <summary>Canonical .xlsx bytes every <c>ChangePricesAndRecalculate</c> benchmark opens.</summary>
    public static byte[] SourceXlsx => LazySourceXlsx.Value;

    /// <summary>
    /// Forces the data generation and the workbook build to happen now, before any measurement.
    /// Every calc benchmark calls this from <c>[GlobalSetup]</c> for the reasons given on
    /// <see cref="StockData.EnsureLoaded"/>.
    /// </summary>
    public static void EnsureLoaded()
    {
        RuntimeHelpers.RunClassConstructor(typeof(CalcData).TypeHandle);
        _ = SourceXlsx;
    }

    static CalcData()
    {
        var random = new Random(Seed);

        Products = new Product[ProductCount];
        for (var i = 0; i < ProductCount; i++)
        {
            Products[i] = new Product(
                Id: $"P{i + 1:000}",
                Category: Categories[random.Next(Categories.Length)],
                UnitPrice: random.Next(5, 501),
                DiscountRate: DiscountRates[random.Next(DiscountRates.Length)]);
        }

        Orders = new Order[OrderCount];
        for (var i = 0; i < OrderCount; i++)
        {
            // i is 0-based, so order (i + 1) is the n-th order in 1-based terms.
            var productId = (i + 1) % UnknownEveryNth == 0
                ? UnknownProductId
                : Products[random.Next(ProductCount)].Id;

            Orders[i] = new Order(
                Id: i + 1,
                ProductId: productId,
                Quantity: random.Next(1, 101),
                Region: Regions[random.Next(Regions.Length)]);
        }

        Expected = CalcExpected.Compute(Products, Orders);
    }

    /// <summary>The price the measured step writes for a product: its original price plus <see cref="PriceIncrease"/>.</summary>
    /// <param name="productIndex">0-based position in <see cref="Products"/>.</param>
    public static double NewPrice(int productIndex) => Products[productIndex].UnitPrice + PriceIncrease;

    // ---- Formulas --------------------------------------------------------------------------

    private static string ProductTable => $"{ProductsSheet}!$A${FirstDataRow}:$D${LastProductRow}";

    private static string ProductColumn(int column)
    {
        var letter = A1.ColumnLetter(column);
        return $"{ProductsSheet}!${letter}${FirstDataRow}:${letter}${LastProductRow}";
    }

    private static string OrderColumn(int column)
    {
        var letter = A1.ColumnLetter(column);
        return $"{OrdersSheet}!${letter}${FirstDataRow}:${letter}${LastOrderRow}";
    }

    public static string UnitPriceFormula(int row) =>
        $"IFERROR(VLOOKUP(B{row},{ProductTable},{UnitPriceCol},FALSE),0)";

    public static string CategoryFormula(int row) =>
        $"IFERROR(INDEX({ProductColumn(ProductCategoryCol)},MATCH(B{row},{ProductColumn(ProductIdCol)},0)),\"{UnknownCategory}\")";

    public static string GrossFormula(int row) => $"C{row}*E{row}";

    public static string DiscountFormula(int row) =>
        $"IF(C{row}>={DiscountQuantity},G{row}*IFERROR(VLOOKUP(B{row},{ProductTable},{DiscountRateCol},FALSE),0),0)";

    public static string NetFormula(int row) => $"ROUND(G{row}-H{row},2)";

    /// <summary>The summary grid cell for one category row and one region column.</summary>
    public static string SumIfsFormula(int row, int column) =>
        $"SUMIFS({OrderColumn(NetCol)},{OrderColumn(OrderCategoryCol)},$A{row},{OrderColumn(RegionCol)},{A1.ColumnLetter(column)}${HeaderRow})";

    public static string CountIfsFormula(int row) => $"COUNTIFS({OrderColumn(OrderCategoryCol)},$A{row})";

    private static byte[] BuildSourceXlsx()
    {
        using var workbook = new XLWorkbook();

        var products = workbook.AddWorksheet(ProductsSheet);
        products.Cell(HeaderRow, ProductIdCol).Value = "ProductId";
        products.Cell(HeaderRow, ProductCategoryCol).Value = "Category";
        products.Cell(HeaderRow, UnitPriceCol).Value = "UnitPrice";
        products.Cell(HeaderRow, DiscountRateCol).Value = "DiscountRate";
        for (var i = 0; i < ProductCount; i++)
        {
            var row = FirstDataRow + i;
            var p = Products[i];
            products.Cell(row, ProductIdCol).Value = p.Id;
            products.Cell(row, ProductCategoryCol).Value = p.Category;
            products.Cell(row, UnitPriceCol).Value = p.UnitPrice;
            products.Cell(row, DiscountRateCol).Value = p.DiscountRate;
        }

        var orders = workbook.AddWorksheet(OrdersSheet);
        string[] orderHeaders =
            ["OrderId", "ProductId", "Quantity", "Region", "UnitPrice", "Category", "Gross", "Discount", "Net"];
        for (var c = 0; c < orderHeaders.Length; c++)
            orders.Cell(HeaderRow, c + 1).Value = orderHeaders[c];
        for (var i = 0; i < OrderCount; i++)
        {
            var row = FirstDataRow + i;
            var o = Orders[i];
            orders.Cell(row, OrderIdCol).Value = o.Id;
            orders.Cell(row, OrderProductCol).Value = o.ProductId;
            orders.Cell(row, QuantityCol).Value = o.Quantity;
            orders.Cell(row, RegionCol).Value = o.Region;
            orders.Cell(row, OrderPriceCol).FormulaA1 = UnitPriceFormula(row);
            orders.Cell(row, OrderCategoryCol).FormulaA1 = CategoryFormula(row);
            orders.Cell(row, GrossCol).FormulaA1 = GrossFormula(row);
            orders.Cell(row, DiscountCol).FormulaA1 = DiscountFormula(row);
            orders.Cell(row, NetCol).FormulaA1 = NetFormula(row);
        }

        var summary = workbook.AddWorksheet(SummarySheet);
        summary.Cell(HeaderRow, 1).Value = "Category";
        for (var r = 0; r < Regions.Length; r++)
            summary.Cell(HeaderRow, FirstRegionCol + r).Value = Regions[r];
        summary.Cell(HeaderRow, CountCol).Value = "Orders";

        for (var i = 0; i < SummaryCategories.Length; i++)
        {
            var row = FirstDataRow + i;
            summary.Cell(row, 1).Value = SummaryCategories[i];
            for (var col = FirstRegionCol; col <= LastRegionCol; col++)
                summary.Cell(row, col).FormulaA1 = SumIfsFormula(row, col);
            summary.Cell(row, CountCol).FormulaA1 = CountIfsFormula(row);
        }

        summary.Cell(TotalsRow, 1).Value = "Total";
        for (var col = FirstRegionCol; col <= CountCol; col++)
        {
            var letter = A1.ColumnLetter(col);
            summary.Cell(TotalsRow, col).FormulaA1 = $"SUM({letter}{FirstDataRow}:{letter}{LastCategoryRow})";
        }

        var grid = $"{A1.ColumnLetter(FirstRegionCol)}{FirstDataRow}:{A1.ColumnLetter(LastRegionCol)}{LastCategoryRow}";
        summary.Cell(GrandTotalRow, 1).Value = "Grand total";
        summary.Cell(GrandTotalRow, StatValueCol).FormulaA1 = $"SUM({grid})";
        summary.Cell(AverageNetRow, 1).Value = "Average net";
        summary.Cell(AverageNetRow, StatValueCol).FormulaA1 = $"AVERAGE({OrderColumn(NetCol)})";
        summary.Cell(MaxNetRow, 1).Value = "Largest net";
        summary.Cell(MaxNetRow, StatValueCol).FormulaA1 = $"MAX({OrderColumn(NetCol)})";

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}

/// <summary>
/// Every value the calc scenario can check, computed in C# from the generated inputs with the
/// prices already raised — the same arithmetic the workbook's formulas describe, done without an
/// engine.
/// </summary>
/// <param name="Net">Each order's <c>Net</c>, indexed by 0-based order.</param>
/// <param name="Category">Each order's looked-up category, indexed by 0-based order.</param>
/// <param name="Grid">Summary <c>SUMIFS</c> values, indexed <c>[category][region]</c>.</param>
/// <param name="Counts">Summary <c>COUNTIFS</c> values, indexed by category.</param>
/// <param name="GrandTotal">The sum of the whole grid — the value every benchmark returns.</param>
/// <param name="AverageNet">The mean of every order's <c>Net</c>.</param>
/// <param name="MaxNet">The largest order <c>Net</c>.</param>
public sealed record CalcExpected(
    double[] Net,
    string[] Category,
    double[][] Grid,
    int[] Counts,
    double GrandTotal,
    double AverageNet,
    double MaxNet)
{
    internal static CalcExpected Compute(CalcData.Product[] products, CalcData.Order[] orders)
    {
        var byId = new Dictionary<string, int>(products.Length, StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < products.Length; i++)
            byId[products[i].Id] = i;

        var categories = CalcData.SummaryCategories;
        var regions = CalcData.Regions;

        var net = new double[orders.Length];
        var category = new string[orders.Length];
        var grid = new double[categories.Length][];
        for (var c = 0; c < grid.Length; c++)
            grid[c] = new double[regions.Length];
        var counts = new int[categories.Length];

        for (var i = 0; i < orders.Length; i++)
        {
            var order = orders[i];
            double price = 0, rate = 0;
            var cat = CalcData.UnknownCategory;
            if (byId.TryGetValue(order.ProductId, out var p))
            {
                price = CalcData.NewPrice(p);
                rate = products[p].DiscountRate;
                cat = products[p].Category;
            }

            var gross = order.Quantity * price;
            var discount = order.Quantity >= CalcData.DiscountQuantity ? gross * rate : 0d;
            net[i] = Math.Round(gross - discount, 2, MidpointRounding.AwayFromZero);
            category[i] = cat;

            var c = Array.IndexOf(categories, cat);
            grid[c][Array.IndexOf(regions, order.Region)] += net[i];
            counts[c]++;
        }

        var grandTotal = grid.Sum(row => row.Sum());
        return new CalcExpected(net, category, grid, counts, grandTotal, net.Average(), net.Max());
    }
}
