using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace XLBench.Data;

/// <summary>
/// The stock dataset backing the <c>CreateStockReport</c> scenario, loaded once from the
/// embedded <c>Data/stock_data.json</c> resource and pivoted into a week × symbol matrix.
///
/// Parsing happens in the static constructor — outside any measured region — because the
/// JSON deserialization is identical for every library and would otherwise add the same
/// constant to every result, diluting the Excel-library differences the benchmark exists
/// to measure.
/// </summary>
public static class StockData
{
    private const string ResourceName = "XLBench.Data.stock_data.json";

    /// <summary>Ticker symbols in first-appearance order — one spreadsheet column each.</summary>
    public static readonly string[] Symbols;

    /// <summary>Company names, parallel to <see cref="Symbols"/>.</summary>
    public static readonly string[] Names;

    /// <summary>Week-ending dates in ascending week order — one spreadsheet row each.</summary>
    public static readonly DateTime[] WeekEndings;

    /// <summary>Closing prices indexed <c>[weekIndex][symbolIndex]</c>.</summary>
    public static readonly double[][] Prices;

    public static int WeekCount => WeekEndings.Length;
    public static int SymbolCount => Symbols.Length;

    /// <summary>
    /// Forces the static constructor — the JSON parse and pivot — to run now.
    /// </summary>
    /// <remarks>
    /// Class initialization is lazy, so without this the load would happen on whichever access
    /// comes first. Under the default job that lands in the pilot or warmup and never reaches the
    /// reported mean, but that is a property of the job rather than of the benchmark: a cold-start
    /// job (<c>--job dry</c>) has no warmup to absorb it, and the cost would be attributed to
    /// whichever library happened to run first. Every report benchmark calls this from
    /// <c>[GlobalSetup]</c> so the dataset is loaded before any measurement, under every job.
    ///
    /// <see cref="RuntimeHelpers.RunClassConstructor"/> rather than touching a member: it states
    /// the intent exactly and cannot be optimized away the way a discarded field read might be.
    /// </remarks>
    public static void EnsureLoaded() =>
        RuntimeHelpers.RunClassConstructor(typeof(StockData).TypeHandle);

    static StockData()
    {
        var records = LoadRecords();

        // Preserve file order for symbols and ascending week order for rows, so every
        // library produces a byte-comparable layout.
        Symbols = records.Select(r => r.TickerSymbol).Distinct().ToArray();
        Names = Symbols
            .Select(s => records.First(r => r.TickerSymbol == s).StockName)
            .ToArray();

        var weekNos = records.Select(r => r.WeekNo).Distinct().Order().ToArray();
        WeekEndings = weekNos
            .Select(w => records.First(r => r.WeekNo == w).WeekEnding)
            .ToArray();

        var symbolIndex = Symbols
            .Select((s, i) => (s, i))
            .ToDictionary(x => x.s, x => x.i);
        var weekIndex = weekNos
            .Select((w, i) => (w, i))
            .ToDictionary(x => x.w, x => x.i);

        Prices = new double[WeekEndings.Length][];
        for (var w = 0; w < Prices.Length; w++)
            Prices[w] = new double[Symbols.Length];

        foreach (var r in records)
            Prices[weekIndex[r.WeekNo]][symbolIndex[r.TickerSymbol]] = r.ClosingPrice;
    }

    private static List<StockRecord> LoadRecords()
    {
        using var stream = typeof(StockData).GetTypeInfo().Assembly
                               .GetManifestResourceStream(ResourceName)
                           ?? throw new InvalidOperationException(
                               $"Embedded resource '{ResourceName}' not found. Ensure Data/stock_data.json " +
                               "is included as an <EmbeddedResource> in XLBench.csproj.");

        var file = JsonSerializer.Deserialize<StockFile>(stream, JsonOptions)
                   ?? throw new InvalidOperationException($"'{ResourceName}' deserialized to null.");

        return file.Records.Count > 0
            ? file.Records
            : throw new InvalidOperationException($"'{ResourceName}' contains no records.");
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private sealed class StockFile
    {
        [JsonPropertyName("records")]
        public List<StockRecord> Records { get; init; } = [];
    }

    private sealed class StockRecord
    {
        [JsonPropertyName("stockName")]
        public string StockName { get; init; } = string.Empty;

        [JsonPropertyName("tickerSymbol")]
        public string TickerSymbol { get; init; } = string.Empty;

        [JsonPropertyName("weekNo")]
        public int WeekNo { get; init; }

        // Serialized as "yyyy-MM-dd"; kept as a string so the JSON contract stays explicit,
        // then projected to a DateTime for the date-formatted spreadsheet column.
        [JsonPropertyName("weekEnding")]
        public string WeekEndingRaw { get; init; } = string.Empty;

        [JsonPropertyName("closingPrice")]
        public double ClosingPrice { get; init; }

        // Not part of the JSON contract — without JsonIgnore it collides with WeekEndingRaw's
        // "weekEnding" mapping under case-insensitive matching.
        [JsonIgnore]
        public DateTime WeekEnding => DateTime.ParseExact(
            WeekEndingRaw, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None);
    }
}
