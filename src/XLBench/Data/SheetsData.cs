using System.Runtime.CompilerServices;
using System.Text;
using ClosedXML.Excel;

namespace XLBench.Data;

/// <summary>
/// The workbook behind the <c>OpenAndReadAllSheets</c> scenario: many sheets whose cost is in the
/// shared parts rather than in the grid — a large shared-string table, rich-text strings and a
/// large style table.
///
/// <para><b>Init (not measured).</b> <see cref="SourceXlsx"/> is a single canonical .xlsx byte
/// buffer built once, lazily, from a seeded generator — <see cref="SheetCount"/> sheets, each a
/// header row then <see cref="RowsPerSheet"/> data rows × <see cref="ColCount"/> columns. Every
/// library's benchmark opens these exact bytes.</para>
///
/// <code>
///        A .. I (text)                          J (number)  K (date)    L (number)
///   1 | Name | Owner | ... | Notes            | Amount    | Due       | Rate        &lt;- header
///   2 | velu toranik | ... | **mira** _sol_   | 4,182.17  | 2023-04-09 | 12.40%
///   ...
///  501 | ...
/// </code>
///
/// <para><b>What makes it different from <see cref="TestData.ReadXlsx"/>.</b> That sheet is one
/// plain grid of mostly unique values with a handful of styles. This one spreads the same order of
/// cells across twenty sheets and makes the workbook-level parts expensive:</para>
/// <list type="bullet">
///   <item>Text cells come from a vocabulary of <see cref="VocabularySize"/> phrases. A share of
///   them come from a small hot set, so the shared-string table has tens of thousands of
///   unique entries <i>and</i> heavy reuse, as real exports do.</item>
///   <item>About one text cell in <see cref="RichTextEveryNth"/> is rich text: 2–4 runs with
///   different fonts. A reader has to join the runs back into the plain text.</item>
///   <item>Every cell takes one of <see cref="StylePaletteSize"/> font/fill/border combinations,
///   and the number and date cells add a number format on top. The <c>cellXfs</c> table ends up
///   with thousands of entries, which an eager model has to parse and index.</item>
/// </list>
///
/// <para><b>Checksum.</b> Libraries render numbers and dates differently, so only the text cells'
/// plain text is compared: <see cref="Checksum"/> combines the number of populated cells with the
/// total length of the text cells. Rich text counts as its runs joined together.
/// <see cref="ExpectedChecksum"/> is computed from the generator's own data.</para>
///
/// <para>The file is generated with ClosedXML purely as a neutral producer of standard OOXML,
/// mirroring <see cref="TestData.ReadXlsx"/>; generation happens outside any measured region so
/// it does not bias results.</para>
/// </summary>
public static class SheetsData
{
    public const int SheetCount = 20;

    /// <summary>Data rows on each sheet, below the header.</summary>
    public const int RowsPerSheet = 500;

    /// <summary>Columns on each sheet: <see cref="TextColCount"/> text columns, then numbers and dates.</summary>
    public const int ColCount = 12;

    /// <summary>The first columns hold text; the rest hold a number, a date and a number.</summary>
    public const int TextColCount = 9;

    /// <summary>Distinct phrases the text cells are drawn from.</summary>
    public const int VocabularySize = 40_000;

    /// <summary>
    /// The first phrases of the vocabulary, which take <see cref="HotShare"/> of all draws. They
    /// give the shared-string table its heavy reuse.
    /// </summary>
    public const int HotSetSize = 250;

    /// <summary>Share of text cells drawn from the hot set instead of the whole vocabulary.</summary>
    public const double HotShare = 0.3;

    /// <summary>On average one text cell in this many is rich text.</summary>
    public const int RichTextEveryNth = 10;

    /// <summary>Font/fill/border combinations a text cell's style is drawn from.</summary>
    public const int StylePaletteSize = 1_200;

    /// <summary>
    /// The number and date cells draw from the first entries of the palette only. Each also adds
    /// one of four number formats per column, so drawing from the whole palette would multiply
    /// the format table past 15,000 entries — beyond what even a heavily styled real workbook
    /// carries.
    /// </summary>
    public const int NumberPaletteSize = 100;

    public const int HeaderRow = 1;
    public const int FirstDataRow = HeaderRow + 1;
    public static int LastDataRow => FirstDataRow + RowsPerSheet - 1;

    /// <summary>The value <see cref="Checksum"/> multiplies the cell count by.</summary>
    /// <remarks>Larger than any total text length the workbook can hold, so the two parts never overlap.</remarks>
    private const long CellCountRadix = 1_000_000_000;

    public static readonly string[] Headers =
        ["Name", "Owner", "Team", "Region", "Product", "Status", "Channel", "Tags", "Notes", "Amount", "Due", "Rate"];

    private static readonly string[] Syllables =
    [
        "ka", "lo", "mi", "ra", "sen", "tor", "vel", "nu", "pa", "dri", "qua", "zen", "ho", "fi", "gar",
        "bel", "co", "da", "esh", "ju", "lin", "mor", "ne", "os", "pri", "rek", "sa", "tu", "vin", "wy",
        "xan", "yo", "zul", "ar", "bo", "cy", "del", "en", "fa", "gu",
    ];

    // Number formats for the three non-text columns. None of them may look like a date format
    // except the date column's, or the streaming readers would hand the numbers back as dates.
    private static readonly string[] AmountFormats = ["#,##0.00", "0.00", "#,##0", "[$$-409]#,##0.00"];
    private static readonly string[] DateFormats = ["yyyy-mm-dd", "dd mmm yyyy", "d/m/yy", "mmm-yy"];
    private static readonly string[] RateFormats = ["0.00%", "0.0%", "0.000", "0.00E+00"];

    private static readonly XLColor[] FontColors =
    [
        XLColor.Black, XLColor.DarkBlue, XLColor.DarkRed, XLColor.DarkGreen, XLColor.Purple, XLColor.SaddleBrown,
    ];

    private static readonly XLColor[] FillColors =
    [
        XLColor.NoColor, XLColor.LightYellow, XLColor.LightBlue, XLColor.LightGreen, XLColor.LightPink,
        XLColor.Lavender, XLColor.Beige, XLColor.Honeydew, XLColor.MistyRose, XLColor.AliceBlue,
    ];

    private static readonly XLBorderStyleValues[] Borders =
    [
        XLBorderStyleValues.None, XLBorderStyleValues.Thin, XLBorderStyleValues.Dotted,
        XLBorderStyleValues.Dashed, XLBorderStyleValues.Medium,
    ];

    private static readonly Lazy<(byte[] Bytes, long Checksum)> LazyWorkbook = new(Build, isThreadSafe: true);

    /// <summary>Canonical .xlsx bytes every <c>OpenAndReadAllSheets</c> benchmark opens.</summary>
    public static byte[] SourceXlsx => LazyWorkbook.Value.Bytes;

    /// <summary>The checksum a correct read of <see cref="SourceXlsx"/> returns.</summary>
    public static long ExpectedChecksum => LazyWorkbook.Value.Checksum;

    /// <summary>Populated cells in the workbook: every cell of every sheet, header included.</summary>
    public static long ExpectedCellCount => (long)SheetCount * (RowsPerSheet + 1) * ColCount;

    /// <summary>
    /// Forces the workbook build to happen now, before any measurement. Every benchmark calls
    /// this from <c>[GlobalSetup]</c> for the reasons given on <see cref="StockData.EnsureLoaded"/>.
    /// </summary>
    public static void EnsureLoaded()
    {
        RuntimeHelpers.RunClassConstructor(typeof(SheetsData).TypeHandle);
        _ = SourceXlsx;
    }

    /// <summary>
    /// The value every benchmark returns: the populated-cell count and the total length of the
    /// text cells' plain text, in one number so a wrong count or a wrong length both show.
    /// </summary>
    public static long Checksum(long cells, long textChars) => (cells * CellCountRadix) + textChars;

    /// <summary>Splits a <see cref="Checksum"/> back into its cell count and text length, for reporting.</summary>
    public static (long Cells, long TextChars) Split(long checksum) =>
        (checksum / CellCountRadix, checksum % CellCountRadix);

    private static (byte[] Bytes, long Checksum) Build()
    {
#pragma warning disable S2245 // Deterministic seed is intentional for reproducible benchmarks
        var random = new Random(42);
#pragma warning restore S2245
        var vocabulary = BuildVocabulary(random);
        var baseDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

        using var workbook = new XLWorkbook();

        // Styling a cell one property at a time re-keys its style on every set, which makes
        // generating ~240,000 styled cells slow. Each palette entry is styled once on a scratch
        // sheet instead, and data cells copy the finished style from it.
        var paletteSheet = workbook.AddWorksheet("Palette");
        var palette = new IXLStyle[StylePaletteSize];
        for (var p = 0; p < StylePaletteSize; p++)
        {
            var style = paletteSheet.Cell(p + 1, 1).Style;
            style.Font.FontColor = FontColors[p % FontColors.Length];
            style.Font.Bold = (p / 6) % 2 == 1;
            style.Font.Italic = (p / 12) % 2 == 1;
            var fill = FillColors[(p / 24) % FillColors.Length];
            if (fill != XLColor.NoColor)
                style.Fill.BackgroundColor = fill;
            // OutsideBorder is a range operation and leaves a lone cell's style untouched, so the
            // four sides are set one by one.
            var border = Borders[(p / 240) % Borders.Length];
            style.Border.LeftBorder = border;
            style.Border.RightBorder = border;
            style.Border.TopBorder = border;
            style.Border.BottomBorder = border;
            palette[p] = style;
        }

        long textChars = 0;

        for (var s = 0; s < SheetCount; s++)
        {
            var ws = workbook.AddWorksheet($"Data {s + 1:D2}");

            for (var c = 0; c < ColCount; c++)
            {
                var header = ws.Cell(HeaderRow, c + 1);
                header.Value = Headers[c];
                header.Style.Font.Bold = true;
                textChars += Headers[c].Length;
            }

            for (var row = FirstDataRow; row <= LastDataRow; row++)
            {
                for (var col = 1; col <= TextColCount; col++)
                {
                    var cell = ws.Cell(row, col);
                    var phrase = random.NextDouble() < HotShare
                        ? vocabulary[random.Next(HotSetSize)]
                        : vocabulary[random.Next(vocabulary.Length)];

                    if (random.Next(RichTextEveryNth) == 0)
                        textChars += WriteRichText(cell, phrase, random);
                    else
                    {
                        cell.Value = phrase;
                        textChars += phrase.Length;
                    }

                    cell.Style = palette[random.Next(StylePaletteSize)];
                }

                var amount = ws.Cell(row, TextColCount + 1);
                amount.Value = Math.Round((random.NextDouble() * 20_000) - 2_000, 2);
                amount.Style = palette[random.Next(NumberPaletteSize)];
                amount.Style.NumberFormat.Format = AmountFormats[random.Next(AmountFormats.Length)];

                var due = ws.Cell(row, TextColCount + 2);
                due.Value = baseDate.AddDays(random.Next(0, 2_000));
                due.Style = palette[random.Next(NumberPaletteSize)];
                due.Style.NumberFormat.Format = DateFormats[random.Next(DateFormats.Length)];

                var rate = ws.Cell(row, TextColCount + 3);
                rate.Value = Math.Round(random.NextDouble() * 0.5, 4);
                rate.Style = palette[random.Next(NumberPaletteSize)];
                rate.Style.NumberFormat.Format = RateFormats[random.Next(RateFormats.Length)];
            }
        }

        // Its cells were only ever style sources; the styles they defined live on in the data.
        paletteSheet.Delete();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return (ms.ToArray(), Checksum(ExpectedCellCount, textChars));
    }

    /// <summary>
    /// Writes <paramref name="phrase"/> as 2–4 runs, each word group in its own font. Returns the
    /// plain-text length, which is the phrase's: the runs join back into it exactly.
    /// </summary>
    private static int WriteRichText(IXLCell cell, string phrase, Random random)
    {
        var words = phrase.Split(' ');
        var runs = Math.Min(words.Length, random.Next(2, 5));

        var rich = cell.CreateRichText();
        var perRun = (int)Math.Ceiling(words.Length / (double)runs);
        for (var start = 0; start < words.Length; start += perRun)
        {
            var count = Math.Min(perRun, words.Length - start);
            var text = string.Join(' ', words, start, count);
            if (start + count < words.Length) text += " ";

            var run = rich.AddText(text);
            switch (random.Next(4))
            {
                case 0: run.SetBold(); break;
                case 1: run.SetItalic(); break;
                case 2: run.SetFontColor(XLColor.Red); break;
                default: run.SetFontSize(14).SetUnderline(); break;
            }
        }
        return phrase.Length;
    }

    /// <summary>
    /// Phrases of 2–5 made-up words. Words are syllable pairs and triples, so the vocabulary is
    /// large without needing a dictionary, and the phrases are realistic lengths for text cells.
    /// </summary>
    private static string[] BuildVocabulary(Random random)
    {
        var words = new List<string>(Syllables.Length * Syllables.Length);
        foreach (var a in Syllables)
        foreach (var b in Syllables)
            words.Add(a + b);

        var phrases = new HashSet<string>(VocabularySize, StringComparer.Ordinal);
        var sb = new StringBuilder();
        while (phrases.Count < VocabularySize)
        {
            sb.Clear();
            var count = random.Next(2, 6);
            for (var w = 0; w < count; w++)
            {
                if (w > 0) sb.Append(' ');
                sb.Append(words[random.Next(words.Count)]);
                if (random.Next(3) == 0) sb.Append(Syllables[random.Next(Syllables.Length)]);
            }
            phrases.Add(sb.ToString());
        }
        return [.. phrases];
    }
}
