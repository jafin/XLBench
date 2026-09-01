---
title: XLBench
---

# .NET Excel Library Performance Benchmarks

<p>
Independent performance and memory benchmarks comparing
ClosedXML, EPPlus, OpenXML SDK, NPOI, MiniExcel, XLibur, IronXL and Telerik.
</p>

Independent read/write **performance and memory** benchmarks comparing popular .NET Excel
libraries, all consumed via NuGet and run on **.NET 10** with
[BenchmarkDotNet](https://benchmarkdotnet.org/).

## 📊 [Latest results (tables + static charts) →](results.md)
## 📈 [Interactive charts →](charts.html)
## 🤖 [CI results (indicative) →](results-ci.md)

## Libraries under test

| Library | Package | Version |
| --- | --- | --- |
| [ClosedXML](https://github.com/ClosedXML/ClosedXML) | [`ClosedXML`](https://www.nuget.org/packages/ClosedXML) | 0.105.1 |
| [EPPlus](https://github.com/EPPlusSoftware/EPPlus) | [`EPPlus`](https://www.nuget.org/packages/EPPlus) | 8.7.0 |
| [OpenXML SDK](https://github.com/dotnet/Open-XML-SDK) | [`DocumentFormat.OpenXml`](https://www.nuget.org/packages/DocumentFormat.OpenXml) | 3.5.1 |
| [NPOI](https://github.com/nissl-lab/npoi) | [`NPOI`](https://www.nuget.org/packages/NPOI) | 2.8.0 |
| [MiniExcel](https://github.com/mini-software/MiniExcel) | [`MiniExcel`](https://www.nuget.org/packages/MiniExcel) | 1.46.0 |
| [XLibur](https://github.com/XLibur/XLibur) | [`XLibur.Bundle`](https://www.nuget.org/packages/XLibur.Bundle) | 0.311.2-alpha.34 |
| [IronXL](https://ironsoftware.com/csharp/excel/) | [`IronXL.Excel`](https://www.nuget.org/packages/IronXL.Excel) | 2026.8.1 |
| [Telerik](https://www.telerik.com/document-processing-libraries) | [`Telerik.Documents.Spreadsheet`](https://www.nuget.org/packages/Telerik.Documents.Spreadsheet) | 2026.3.826 |

Library links point at each project's source repository, except IronXL and Telerik, which are
closed source — those go to their product pages.

IronXL and Telerik are commercial. IronXL needs a licence key to run at all and is otherwise
replayed from a committed snapshot (marked ⧗); Telerik runs unlicensed but watermarks every
workbook it writes, so it is excluded from keyless runs — including the CI page — rather than
measured. See the [README](https://github.com/jafin/XLBench#libraries-under-test).

## Scenarios

- **Read** — `OpenAndReadAll` (open + read every cell) over a 50,000 × 15 sheet; every library
  reads the exact same `.xlsx` bytes. Alongside it, `OpenAmendPropertiesAndSave` is a metadata
  round trip on a smaller purpose-built 1,000 × 8 numeric sheet: open, set the document `Title`
  and `Category`, save back out. It uses the smaller workbook on purpose — at 750,000 cells the
  serialization would bury the part being measured.
- **Write** — `CreateAndSave` builds a 50,000-row sheet (string / number / date columns
  plus a `SUM` total) and serializes it to a stream.
- **Report** — `CreateStockReport` imports 20 tickers × 260 weekly closing prices from JSON
  (five years, 5,200 records), lays them out as a 261 × 22 sheet, conditionally formats every
  price green or red against the prior week's close, auto-fits the week-ending column, and
  plots all 20 symbols as a line chart. Feature-bound, not volume-bound.
- **Edit** — `EditAndRecalculate` opens a prepared 500 × 20 workbook (each row totalled by a
  `SUM(A:T)` in column U, every second row bold), deletes every third data row, sets column A to
  `20` on every survivor, and recalculates the row totals. Deleting a row means shifting
  everything below it *and* rewriting their `SUM` ranges, so this is where the cost of
  maintaining a cell model shows up. Every library produces the same 335-row result.
- **Insert** — `InsertColumnsAndRecalculate` opens that same prepared workbook, deletes nothing,
  inserts 2 columns before column B, writes `10` into both on all 500 data rows, and recalculates.
  The new columns land *inside* the totalled range, so every `SUM(A:T)` has to come back as
  `SUM(A:V)` and take them in. Where the edit scenario re-points every formula 166 times, this is
  one structural edit with a single workbook-wide reference fixup — read the two together.

## Report scenario — capability matrix

Not every library can express this scenario; a library that skips a feature is doing strictly
less work, so read its timing against this table.

| Library | Import + grid | Conditional formatting | Auto-fit column | Chart |
| --- | :-: | :-: | :-: | :-: |
| ClosedXML | ✅ | ✅ | ✅ | ❌ no public API |
| EPPlus | ✅ | ✅ | ✅ | ✅ |
| OpenXML SDK | ✅ | ⚠️ hand-authored | ⚠️ estimated width | ⚠️ hand-authored |
| NPOI | ✅ | ✅ | ✅ | ⚠️ titled, invalid XML |
| MiniExcel | ✅ | ❌ | ❌ | ❌ (not benchmarked) |
| XLibur | ✅ | ✅ | ✅ | ✅ |
| IronXL | ✅ | ⚠️ font colour only | ✅ | ✅ |
| Telerik | ✅ | ✅ | ✅ | ✅ |

## Edit scenario — capability matrix

| Library | Open + edit in place | Delete row (shift + reference fixup) | Calculation engine |
| --- | :-: | :-: | :-: |
| ClosedXML | ✅ | ✅ `IXLRow.Delete()` | ✅ |
| EPPlus | ✅ | ✅ `DeleteRow()` | ✅ |
| OpenXML SDK | ⚠️ hand-authored | ⚠️ hand-authored | ❌ sums computed by the benchmark |
| NPOI | ✅ | ⚠️ `RemoveRow` + `ShiftRows` | ✅ |
| MiniExcel | ❌ | ❌ | ❌ (not benchmarked) |
| XLibur | ✅ | ✅ `IXLRow.Delete()` | ✅ |
| IronXL | ✅ | ✅ `RemoveRow()` | ✅ |
| Telerik | ✅ | ✅ `RowSelection.Remove()` | ✅ |

## Insert scenario — capability matrix

| Library | Open + edit in place | Insert columns (shift + reference fixup) | Calculation engine |
| --- | :-: | :-: | :-: |
| ClosedXML | ✅ | ✅ `IXLColumn.InsertColumnsBefore()` | ✅ |
| EPPlus | ✅ | ✅ `InsertColumn(from, count)` | ✅ |
| OpenXML SDK | ⚠️ hand-authored | ⚠️ hand-authored | ❌ sums computed by the benchmark |
| NPOI | ✅ | ✅ `XSSFSheet.ShiftColumns()` | ✅ |
| MiniExcel | ❌ | ❌ | ❌ (not benchmarked) |
| XLibur | ✅ | ✅ `IXLColumn.InsertColumnsBefore()` | ✅ |
| IronXL | ✅ | ✅ `InsertColumns(index, count)` | ✅ |
| Telerik | ✅ | ✅ `ColumnSelection.Insert()` | ✅ |

All 500 row totals in every saved artifact are checked against the source CSV on each
`dotnet run -- insert`, read out of the package XML rather than through a library.

## Caveats

- **Timings are hardware-specific.** The allocation and GC columns are the most portable
  signal when comparing across machines.
- OpenXML SDK and MiniExcel are streaming APIs with no eager "load workbook" step, so they
  only appear in the read-all scenario.
- Telerik's `Workbook.DocumentInfo` has no `Category`, so its properties round trip writes the
  Title only — one property fewer than the other five.
- The other five libraries in the properties round trip persist both properties, but not to the
  same place. Four write the OPC core-properties part the package already pointed at (`.psmdcp`);
  EPPlus writes the conventional `docProps/core.xml` and leaves the inherited relationship
  behind, so its output declares two core-properties relationships where OPC allows one. Excel
  reads it; a strict relationship-following reader gets the part without the title. See the
  [README](https://github.com/jafin/XLBench#fairness-notes).
- MiniExcel has no formula engine; its write total is a pre-computed value rather than a
  `SUM()` formula. It supports neither conditional formatting nor charts, so it sits out the
  report scenario entirely, and it has no cell model to open and mutate, so it sits out the
  edit and insert scenarios too. All other differences are noted inline in the source.
- Telerik's report row has a wide error bar by nature, not by undersampling: the scenario
  allocates ~430 MB per operation and two gen2 collections come with it, so iteration times
  scatter by ~16% however long the run. It is still roughly 5x the next slowest library there,
  so the ranking holds — read it as "around 160 ms, ±25".
- The OpenXML SDK has no calculation engine, so in the edit and insert scenarios its
  "recalculation" is a sum the benchmark computes and writes into the cached value itself. It is
  also the only library there doing a single ordered pass with no model to maintain — read its
  timing with both facts in mind.
- ClosedXML is the only library that does not persist what it recalculated: a plain `SaveAs`
  writes each formula cell with no cached value, leaving the totals to Excel on open. That
  affects the saved artifacts, not the timings — the insert scenario's artifact save opts into
  `evaluateFormulae: true` so its file carries the numbers like the others do.
- **IronXL is licence-gated.** It is commercial and cannot run without a key, so a run that
  lacks one replays its rows and chart points from a previously captured run instead — marked
  ⧗, and named with the version and capture date it came from. The current results carry no
  such marks: every number on the page, IronXL included, was measured in one run on one
  machine. See the
  [README](https://github.com/jafin/XLBench#ironxl--licence-gated-and-snapshotted) for detail.

Results are produced by running the suite locally and committing the generated markdown —
see the [repository README](https://github.com/jafin/XLBench) for how to reproduce.
