---
title: XLBench
---

# XLBench — .NET Excel Library Benchmarks

Independent read/write **performance and memory** benchmarks comparing popular .NET Excel
libraries, all consumed via NuGet and run on **.NET 10** with
[BenchmarkDotNet](https://benchmarkdotnet.org/).

## 📊 [View the latest results →](results.md)

## Libraries under test

| Library | Package |
| --- | --- |
| ClosedXML | `ClosedXML` |
| EPPlus | `EPPlus` |
| OpenXML SDK | `DocumentFormat.OpenXml` |
| NPOI | `NPOI` |
| MiniExcel | `MiniExcel` |
| XLibur | `XLibur.Bundle` |

## Scenarios

- **Read** — `OpenWorkbook` (load into memory) and `OpenAndReadAll` (open + read every cell)
  over a 250,000 × 15 sheet. Every library reads the exact same `.xlsx` bytes.
- **Write** — `CreateAndSave` builds a 50,000-row sheet (string / number / date columns
  plus a `SUM` total) and serializes it to a stream.

## Caveats

- **Timings are hardware-specific.** The allocation and GC columns are the most portable
  signal when comparing across machines.
- OpenXML SDK and MiniExcel are streaming APIs with no eager "load workbook" step, so they
  only appear in the read-all scenario.
- MiniExcel has no formula engine; its write total is a pre-computed value rather than a
  `SUM()` formula. All other differences are noted inline in the source.

Results are produced by running the suite locally and committing the generated markdown —
see the [repository README](https://github.com/jafin/XLBench) for how to reproduce.
