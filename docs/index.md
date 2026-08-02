---
title: XLBench
---

# XLBench — .NET Excel Library Benchmarks

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
| [EPPlus](https://github.com/EPPlusSoftware/EPPlus) | [`EPPlus`](https://www.nuget.org/packages/EPPlus) | 8.6.3 |
| [OpenXML SDK](https://github.com/dotnet/Open-XML-SDK) | [`DocumentFormat.OpenXml`](https://www.nuget.org/packages/DocumentFormat.OpenXml) | 3.5.1 |
| [NPOI](https://github.com/nissl-lab/npoi) | [`NPOI`](https://www.nuget.org/packages/NPOI) | 2.8.0 |
| [MiniExcel](https://github.com/mini-software/MiniExcel) | [`MiniExcel`](https://www.nuget.org/packages/MiniExcel) | 1.45.0 |
| [XLibur](https://github.com/XLibur/XLibur) | [`XLibur.Bundle`](https://www.nuget.org/packages/XLibur.Bundle) | 0.300.0 |
| [IronXL](https://ironsoftware.com/csharp/excel/) | [`IronXL.Excel`](https://www.nuget.org/packages/IronXL.Excel) | 2026.8.1 |

Library links point at each project's source repository, except IronXL, which is closed source —
that one goes to the product page.

## Scenarios

- **Read** — `OpenWorkbook` (load into memory) and `OpenAndReadAll` (open + read every cell)
  over a 50,000 × 15 sheet. Every library reads the exact same `.xlsx` bytes.
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

## Caveats

- **Timings are hardware-specific.** The allocation and GC columns are the most portable
  signal when comparing across machines.
- OpenXML SDK and MiniExcel are streaming APIs with no eager "load workbook" step, so they
  only appear in the read-all scenario.
- MiniExcel has no formula engine; its write total is a pre-computed value rather than a
  `SUM()` formula. It supports neither conditional formatting nor charts, so it sits out the
  report scenario entirely, and it has no cell model to open and mutate, so it sits out the
  edit scenario too. All other differences are noted inline in the source.
- The OpenXML SDK has no calculation engine, so in the edit scenario its "recalculation" is a
  sum the benchmark computes and writes into the cached value itself. It is also the only
  library there doing a single ordered pass with no model to maintain — read its timing with
  both facts in mind.
- **IronXL is licence-gated.** It is commercial and cannot run without a key, so a run that
  lacks one replays its rows and chart points from a previously captured run instead — marked
  ⧗, and named with the version and capture date it came from. The current results carry no
  such marks: every number on the page, IronXL included, was measured in one run on one
  machine. See the
  [README](https://github.com/jafin/XLBench#ironxl--licence-gated-and-snapshotted) for detail.

Results are produced by running the suite locally and committing the generated markdown —
see the [repository README](https://github.com/jafin/XLBench) for how to reproduce.
