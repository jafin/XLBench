# XLBench

Independent **performance and memory** benchmarks for .NET Excel libraries, consumed via
NuGet and run on **.NET 10** with [BenchmarkDotNet](https://benchmarkdotnet.org/). Results
are published as GitHub-flavored markdown to **GitHub Pages**.

📊 **Results:** https://jafin.github.io/XLBench/

## Libraries under test

| Library | NuGet package | Version | Notes |
| --- | --- | --- | --- |
| ClosedXML | `ClosedXML` | 0.105.1 | High-level cell model |
| EPPlus | `EPPlus` | 8.6.3 | Requires a license declaration (non-commercial, set in code) |
| OpenXML SDK | `DocumentFormat.OpenXml` | 3.5.1 | Low-level SAX streaming |
| NPOI | `NPOI` | 2.8.0 | Java POI port |
| MiniExcel | `MiniExcel` | 1.45.0 | Streaming, POCO/dynamic oriented |
| XLibur | `XLibur.Bundle` | 0.106.1-beta.80 | Prerelease; bundles the SkiaSharp font engine (auto-registers). Ahead of stable 0.106.0 for the chart fixes the report scenario needs |

Versions are the pinned NuGet package versions (see `src/XLBench/XLBench.csproj`); the
generated `docs/results.md` reports the exact resolved versions from each run via reflection.

## Scenarios

**Read** (100,000 × 15 sheet — every library reads the *same* `.xlsx` bytes):

- `OpenWorkbook` — load the workbook into memory (eager-model libraries only).
- `OpenAndReadAll` — open, then read every populated cell as a string using each library's
  idiomatic iteration (e.g. ClosedXML/XLibur `CellsUsed()`, EPPlus `Cells`, NPOI row
  enumeration, OpenXML/MiniExcel streaming). Random `Cell(row,col)` indexer access is
  deliberately avoided — it is pathologically slow in some libraries and unrepresentative
  of real usage.

**Write** (`CreateAndSave` — 50,000 rows of string/number/date + a `SUM` total):

- Each library builds and serializes the sheet to a `MemoryStream`.

**Report** (`CreateStockReport` — a small sheet exercising *features* rather than volume):

- Imports `src/XLBench/Data/stock_data.json` (20 tickers × 52 weekly closes) and pivots it
  into a 53-row × 22-column sheet: a header row plus 52 week rows, and two label columns
  (`Week`, `Week Ending`) followed by one price column per symbol — a 52 × 20 price block
  in `C2:V53`.
- Adds **conditional formatting** over that price block as one pair of relative-reference
  expression rules — green when a week closes above the prior week, red when below. The
  applied range is `C3:V53`: week 1 is excluded, having no prior week to compare against.
- **Auto-fits** the week-ending column to its contents. This is the one step whose cost is
  text measurement rather than XML assembly, so it separates libraries that ship a font engine
  from the raw SDK, which has none.
- Adds a **line chart** plotting all 20 symbols against the week-ending dates.

Unlike Read/Write, this scenario is deliberately *not* volume-bound — at 1,166 cells the
timings are dominated by how each library models styles, rules and DrawingML, not by
throughput. Its real output is the capability matrix below.

Every benchmark uses `[MemoryDiagnoser]`, so allocations and Gen0/1/2 collections are
reported alongside timings. A joined summary adds a **Library** column so libraries line up
per scenario.

### Report scenario — capability matrix

Not every library can express this scenario. Read the timings alongside this table: a library
that skips a feature is doing strictly less work.

| Library | Import + grid | Conditional formatting | Auto-fit column | Chart | Artifact valid? |
| --- | :-: | :-: | :-: | :-: | --- |
| ClosedXML | ✅ | ✅ | ✅ | ❌ | ✅ schema-clean |
| EPPlus | ✅ | ✅ | ✅ | ✅ | ✅ schema-clean |
| OpenXML SDK | ✅ | ⚠️ manual | ⚠️ estimated | ⚠️ manual | ✅ schema-clean |
| NPOI | ✅ | ✅ | ✅ | ⚠️ no title | ✅ schema-clean |
| MiniExcel | ✅ | ❌ | ❌ | ❌ | — not benchmarked |
| XLibur | ✅ | ✅ | ✅ | ✅ | ✅ schema-clean |

All four charts carry a legend on the right. EPPlus adds one by default; NPOI, the OpenXML SDK
and XLibur are each asked for one explicitly.

- **ClosedXML — no charts.** 0.105.0 ships internal `XLChart`/`XLCharts` types, but nothing
  exposes them: `IXLWorksheet` has no `Charts` member, so there is no public API to add one.
  Its `CreateStockReport` number therefore covers data + conditional formatting only.
- **OpenXML SDK — everything by hand.** It can do the whole scenario, but the stylesheet,
  the differential formats, the `conditionalFormatting` element, the drawing anchor and the
  entire chart part are authored element by element. That authoring cost — not the runtime —
  is the finding.
- **OpenXML SDK — no auto-fit.** The raw SDK ships no font or text-measurement engine, so there
  is nothing to ask for a fitted width. The benchmark computes one from the longest string the
  column holds and writes an explicit `<col width=…>`. That is an estimate, not auto-fit, and it
  is also why its `CreateStockReport` number skips the text-shaping work the other four do.
- **NPOI — chart title omitted.** `XDDFChart.SetTitleText` serializes the title body as
  `<a:rich>` where the chart schema requires `<c:rich>`, producing a file Excel offers to
  repair. `CT_Tx.Write` passes the `a` prefix unconditionally, so no public API avoids it;
  the chart is emitted without a title so the artifact stays openable. Separately,
  `XDDFLineChartData.SetGrouping` throws `NullReferenceException` on a chart NPOI itself
  created, so `<c:grouping>` is set through the underlying CT model.
- **MiniExcel — excluded.** It has neither conditional formatting nor charts, so it could only
  run the data-import third of the scenario. Timing that against libraries doing all three
  would be actively misleading, so it is left out rather than listed as a fast result.
- **XLibur — pinned to a prerelease for this scenario.** Stable 0.106.0 wrote every series name
  as a `<c:strRef>` with no required `<c:f>` (20 schema errors, one per series) and had no
  legend API at all. Both are fixed in `0.106.1-beta.80`, which is what this repo now pins;
  on it XLibur produces a schema-clean chart with a legend. Legends are opt-in there — a chart
  XLibur creates has none until `Legend.Visible` is set.

### Reviewable output

The report benchmarks keep their workbooks. Each run writes `output/stock-report-<library>.xlsx`
(git-ignored, overwritten on the next run) so the result can be opened and eyeballed:

```pwsh
# Just write the artifacts — same code path, no measurement (seconds, not minutes).
dotnet run -c Release --project src/XLBench -- report
```

Saving happens in `[GlobalCleanup]`, once per library and outside every measured iteration, by
re-running the build against a `FileStream` — so file I/O never lands in the timings. If a
workbook is still open in Excel the save is skipped with a warning rather than failing the run.

### Fairness notes

- OpenXML SDK and MiniExcel are streaming APIs with no eager "load workbook" step, so they
  only appear in `OpenAndReadAll`.
- MiniExcel has no formula engine; its write total is a pre-computed value, not a `SUM()`.
- The shared read file is generated once with ClosedXML purely as a neutral OOXML producer,
  outside any measured region.
- The report scenario's JSON is parsed once into a shared week × symbol matrix, also outside
  any measured region — the deserialization is identical for every library and would only add
  the same constant to each result.
- Report timings are only comparable within the capability matrix above. ClosedXML emits no
  chart, NPOI no chart title, and the OpenXML SDK estimates its column width instead of
  measuring text, so each does less work than a full run of the scenario.
- Auto-fit widths legitimately differ between libraries, because each measures text with its own
  font engine and padding rule: ClosedXML 12.71, XLibur 13.00, EPPlus 14.53, NPOI 11.16, and the
  OpenXML SDK's character-count estimate 11.71. A like-for-like width was not forced — the point
  is what each library does when simply asked to fit the column.
- XLibur is pinned to a prerelease (`0.106.1-beta.80`) rather than stable `0.106.0`, because the
  stable build cannot produce a valid chart for this scenario. That pin applies to the whole
  package, so every XLibur number — read and write included — comes from the prerelease, not
  from the latest stable release the other libraries are on.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (pinned via `global.json`).
- Linux only: `libfontconfig1` (needed by XLibur's SkiaSharp font engine).

## Running

```pwsh
# Full suite (writes docs/results.md) — takes a while at full fidelity.
./scripts/run-benchmarks.ps1

# Subset / faster run
./scripts/run-benchmarks.ps1 -Filter '*Write*'
./scripts/run-benchmarks.ps1 -Filter '*Read*' -Job short
```

Or directly:

```pwsh
dotnet run -c Release --project src/XLBench -- --filter '*'
dotnet run -c Release --project src/XLBench -- --filter '*ClosedXml*' --job short
```

The run publishes a curated `docs/results.md` (raw BenchmarkDotNet output under
`BenchmarkDotNet.Artifacts/` is git-ignored). Commit and push `docs/` to update Pages.

## Publishing (GitHub Pages)

Pages is served from the **`main` branch `/docs` folder**
(Settings → Pages → Source: *Deploy from a branch* → `main` / `/docs`). Because CI runners
are noisy, **authoritative numbers come from local runs you commit** — CI only validates
that the suite builds and executes (see `.github/workflows/build.yml`). An optional on-demand
full run lives in `.github/workflows/benchmark.yml`.

## Adding a library

1. Add the NuGet `PackageReference` to `src/XLBench/XLBench.csproj`.
2. Add `Read/<Name>ReadBenchmarks.cs`, `Write/<Name>WriteBenchmarks.cs` and (where the library
   can express it) `Report/<Name>ReportBenchmarks.cs`, mirroring an existing set. Method names
   must match — `OpenWorkbook` / `OpenAndReadAll` / `CreateAndSave` / `CreateStockReport` — so
   the joined summary aligns.
3. Add a case to `LibraryNameColumn` in `src/XLBench/Config/LibraryComparisonConfig.cs`.
4. For a report benchmark, register it in `Data/ReportArtifacts.cs` so `dotnet run -- report`
   writes its workbook, and add a row to the capability matrix above.

## Dependency updates

[Dependabot](.github/dependabot.yml) opens weekly PRs for outdated NuGet packages (the
Excel libraries, BenchmarkDotNet, tooling) and GitHub Actions. Minor/patch bumps are
grouped into one PR; majors get their own. `build.yml` validates each PR (build + a Dry
smoke run). XLibur is a prerelease, so Dependabot tracks newer `rc` releases.

> **Note:** merging a Dependabot PR updates the *package version and confirms it builds* —
> it does **not** refresh the published benchmark numbers. After merging a library bump,
> re-run `scripts/run-benchmarks.ps1` (or dispatch `benchmark.yml`) and commit the
> regenerated `docs/` so the results reflect the new version.

## Project layout

```
src/XLBench/
  Program.cs                     # switcher + results publisher (-> docs/)
  Config/LibraryComparisonConfig # joined summary, Library column, memory diagnoser, GH export
  Data/TestData.cs               # shared deterministic dataset (seed 42)
  Libraries/EpPlusLicense.cs     # EPPlus non-commercial license declaration
  Benchmarks/Read/*              # one class per library
  Benchmarks/Write/*             # one class per library
docs/                            # GitHub Pages content (index.md + generated results.md)
scripts/run-benchmarks.ps1
```
