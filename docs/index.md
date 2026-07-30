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
| ClosedXML | `ClosedXML` | 0.105.1 |
| EPPlus | `EPPlus` | 8.6.3 |
| OpenXML SDK | `DocumentFormat.OpenXml` | 3.5.1 |
| NPOI | `NPOI` | 2.8.0 |
| MiniExcel | `MiniExcel` | 1.45.0 |
| XLibur | `XLibur.Bundle` | 0.106.1-beta.80 |

## Scenarios

- **Read** — `OpenWorkbook` (load into memory) and `OpenAndReadAll` (open + read every cell)
  over a 100,000 × 15 sheet. Every library reads the exact same `.xlsx` bytes.
- **Write** — `CreateAndSave` builds a 50,000-row sheet (string / number / date columns
  plus a `SUM` total) and serializes it to a stream.
- **Report** — `CreateStockReport` imports 20 tickers × 52 weekly closing prices from JSON,
  lays them out as a 53 × 22 sheet, conditionally formats every price green or red against the
  prior week's close, auto-fits the week-ending column, and plots all 20 symbols as a line
  chart. Feature-bound, not volume-bound.

## Report scenario — capability matrix

Not every library can express this scenario; a library that skips a feature is doing strictly
less work, so read its timing against this table.

| Library | Import + grid | Conditional formatting | Auto-fit column | Chart |
| --- | :-: | :-: | :-: | :-: |
| ClosedXML | ✅ | ✅ | ✅ | ❌ no public API |
| EPPlus | ✅ | ✅ | ✅ | ✅ |
| OpenXML SDK | ✅ | ⚠️ hand-authored | ⚠️ estimated width | ⚠️ hand-authored |
| NPOI | ✅ | ✅ | ✅ | ⚠️ title omitted |
| MiniExcel | ✅ | ❌ | ❌ | ❌ (not benchmarked) |
| XLibur | ✅ | ✅ | ✅ | ✅ |

## Caveats

- **Timings are hardware-specific.** The allocation and GC columns are the most portable
  signal when comparing across machines.
- OpenXML SDK and MiniExcel are streaming APIs with no eager "load workbook" step, so they
  only appear in the read-all scenario.
- MiniExcel has no formula engine; its write total is a pre-computed value rather than a
  `SUM()` formula. It supports neither conditional formatting nor charts, so it sits out the
  report scenario entirely. All other differences are noted inline in the source.

Results are produced by running the suite locally and committing the generated markdown —
see the [repository README](https://github.com/jafin/XLBench) for how to reproduce.
