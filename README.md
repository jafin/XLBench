# XLBench

Independent **performance and memory** benchmarks for .NET Excel libraries, consumed via
NuGet and run on **.NET 10** with [BenchmarkDotNet](https://benchmarkdotnet.org/). Results
are published as GitHub-flavored markdown to **GitHub Pages**.

📊 **Results:** https://jafin.github.io/XLBench/

## Libraries under test

| Library | NuGet package | Notes |
| --- | --- | --- |
| ClosedXML | `ClosedXML` | High-level cell model |
| EPPlus | `EPPlus` | Requires a license declaration (non-commercial, set in code) |
| OpenXML SDK | `DocumentFormat.OpenXml` | Low-level SAX streaming |
| NPOI | `NPOI` | Java POI port |
| MiniExcel | `MiniExcel` | Streaming, POCO/dynamic oriented |
| XLibur | `XLibur.Bundle` | Prerelease; bundles the SkiaSharp font engine (auto-registers) |

## Scenarios

**Read** (200,000 × 15 sheet — every library reads the *same* `.xlsx` bytes):

- `OpenWorkbook` — load the workbook into memory (eager-model libraries only).
- `OpenAndReadAll` — open, then read every populated cell as a string using each library's
  idiomatic iteration (e.g. ClosedXML/XLibur `CellsUsed()`, EPPlus `Cells`, NPOI row
  enumeration, OpenXML/MiniExcel streaming). Random `Cell(row,col)` indexer access is
  deliberately avoided — it is pathologically slow in some libraries and unrepresentative
  of real usage.

**Write** (`CreateAndSave` — 50,000 rows of string/number/date + a `SUM` total):

- Each library builds and serializes the sheet to a `MemoryStream`.

Every benchmark uses `[MemoryDiagnoser]`, so allocations and Gen0/1/2 collections are
reported alongside timings. A joined summary adds a **Library** column so libraries line up
per scenario.

### Fairness notes

- OpenXML SDK and MiniExcel are streaming APIs with no eager "load workbook" step, so they
  only appear in `OpenAndReadAll`.
- MiniExcel has no formula engine; its write total is a pre-computed value, not a `SUM()`.
- The shared read file is generated once with ClosedXML purely as a neutral OOXML producer,
  outside any measured region.

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
2. Add `Read/<Name>ReadBenchmarks.cs` and `Write/<Name>WriteBenchmarks.cs` mirroring an
   existing pair (method names must match — `OpenWorkbook` / `OpenAndReadAll` / `CreateAndSave`
   — so the joined summary aligns).
3. Add a case to `LibraryNameColumn` in `src/XLBench/Config/LibraryComparisonConfig.cs`.

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
