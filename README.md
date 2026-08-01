# XLBench

Independent **performance and memory** benchmarks for .NET Excel libraries, consumed via
NuGet and run on **.NET 10** with [BenchmarkDotNet](https://benchmarkdotnet.org/). Results
are published as GitHub-flavored markdown to **GitHub Pages**.

📊 **Results:** https://jafin.github.io/XLBench/

## Libraries under test

| Library | NuGet package | Version | Notes |
| --- | --- | --- | --- |
| [ClosedXML](https://github.com/ClosedXML/ClosedXML) | [`ClosedXML`](https://www.nuget.org/packages/ClosedXML) | 0.105.1 | High-level cell model |
| [EPPlus](https://github.com/EPPlusSoftware/EPPlus) | [`EPPlus`](https://www.nuget.org/packages/EPPlus) | 8.6.3 | Requires a license declaration (non-commercial, set in code) |
| [OpenXML SDK](https://github.com/dotnet/Open-XML-SDK) | [`DocumentFormat.OpenXml`](https://www.nuget.org/packages/DocumentFormat.OpenXml) | 3.5.1 | Low-level SAX streaming |
| [NPOI](https://github.com/nissl-lab/npoi) | [`NPOI`](https://www.nuget.org/packages/NPOI) | 2.8.0 | Java POI port |
| [MiniExcel](https://github.com/mini-software/MiniExcel) | [`MiniExcel`](https://www.nuget.org/packages/MiniExcel) | 1.45.0 | Streaming, POCO/dynamic oriented |
| [XLibur](https://github.com/XLibur/XLibur) | [`XLibur.Bundle`](https://www.nuget.org/packages/XLibur.Bundle) | 0.200.0 | Bundles the SkiaSharp font engine (auto-registers) |
| [IronXL](https://ironsoftware.com/csharp/excel/) | [`IronXL.Excel`](https://www.nuget.org/packages/IronXL.Excel) | 2026.8.1 | **Commercial.** Runs only with a licence key; otherwise its results are replayed from `snapshots/` — see [IronXL](#ironxl--licence-gated-and-snapshotted) |

Library links point at each project's source repository, except IronXL, which is closed source —
that one goes to the product page. Versions are the pinned NuGet package versions (see
`src/XLBench/XLBench.csproj`); the generated `docs/results.md` reports the exact resolved
versions from each run via reflection.

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

- Imports `src/XLBench/Data/stock_data.json` (20 tickers × 260 weekly closes — five years,
  5,200 records) and pivots it into a 261-row × 22-column sheet: a header row plus 260 week
  rows, and two label columns (`Week`, `Week Ending`) followed by one price column per symbol
  — a 260 × 20 price block in `C2:V261`.
- Adds **conditional formatting** over that price block as one pair of relative-reference
  expression rules — green when a week closes above the prior week, red when below. The
  applied range is `C3:V261`: week 1 is excluded, having no prior week to compare against.
- **Auto-fits** the week-ending column to its contents. This is the one step whose cost is
  text measurement rather than XML assembly, so it separates libraries that ship a font engine
  from the raw SDK, which has none.
- Adds a **line chart** plotting all 20 symbols against the week-ending dates.

Unlike Read/Write, this scenario is deliberately *not* volume-bound — at 5,742 cells the
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
| NPOI | ✅ | ✅ | ✅ | ✅ | ⚠️ 1 schema error |
| MiniExcel | ✅ | ❌ | ❌ | ❌ | — not benchmarked |
| XLibur | ✅ | ✅ | ✅ | ✅ | ✅ schema-clean |
| IronXL | ✅ | ⚠️ font only | ✅ | ✅ | ⚠️ 1 schema error |

All five charts carry a legend on the right. EPPlus adds one by default; NPOI, the OpenXML SDK,
XLibur and IronXL are each asked for one explicitly.

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
- **NPOI — titled chart, at the cost of a schema-invalid artifact.** `XDDFChart.SetTitleText`
  serializes the title body as `<a:rich>` where the chart schema requires `<c:rich>`, and
  `CT_Tx.Write` passes the `a` prefix unconditionally, so no public API avoids it. The title is
  set anyway — so NPOI does the same work as the other libraries and its timing stays
  comparable — which leaves `output/stock-report-npoi.xlsx` with one validation error, and
  Excel may offer to repair it. Dropping `SetTitleText` (or re-adding
  `SetAutoTitleDeleted(true)`, which discards the title NPOI just built) restores a clean file
  and an untitled chart. Separately, `XDDFLineChartData.SetGrouping` throws
  `NullReferenceException` on a chart NPOI itself created, so `<c:grouping>` is set through the
  underlying CT model.
- **MiniExcel — excluded.** It has neither conditional formatting nor charts, so it could only
  run the data-import third of the scenario. Timing that against libraries doing all three
  would be actively misleading, so it is left out rather than listed as a fast result.
- **XLibur — legends are opt-in.** A chart XLibur creates has none until `Legend.Visible` is
  set, so the report benchmark sets it explicitly. (This scenario used to force a prerelease
  pin: stable `0.106.0` wrote every series name as a `<c:strRef>` with no required `<c:f>` —
  20 schema errors, one per series — and had no legend API at all. Both fixes have since
  shipped in stable, so the repo tracks the stable channel again from `0.200.0`.)

### IronXL — licence-gated and snapshotted

IronXL is implemented for all three scenarios, but only measures when `XLBENCH_IRONXL_KEY` holds
a valid licence key. Runs without one replay the committed `snapshots/ironxl.json` instead, so
IronXL stays in the comparison without every contributor needing a key. The findings below were
produced under a time-limited trial key and are reproducible with any valid key.

#### Snapshots — how IronXL still appears in the results

Requiring a key for every run would drop IronXL out of the comparison entirely, so its results
are **carried between runs**:

- A run **with** `XLBENCH_IRONXL_KEY` measures IronXL normally and writes what it measured to
  `snapshots/ironxl.json` — timings, allocations, and the markdown row BenchmarkDotNet rendered,
  each stamped with the IronXL version, host, job and capture time.
- A run **without** the key skips IronXL and replays that snapshot into the results tables, the
  static charts, the interactive page and the versions table, every occurrence marked **⧗** with
  a banner naming the version and date the numbers came from.

Entries are merged per method, so `--filter '*Read*'` with a key refreshes only the read methods
and leaves the rest of the snapshot intact. Two guards stop a stale snapshot producing a wrong
table row: the column set must still match, and the `Mean` column must be in the same unit
(BenchmarkDotNet picks units per table). If either differs the row is dropped with a warning,
though the chart data — which is stored numerically in ms — is still used.

```pwsh
# Refresh the snapshot after an IronXL version bump.
$env:XLBENCH_IRONXL_KEY = 'IRONXL.YOURCOMPANY.IRO######.####'
./scripts/run-benchmarks.ps1
# then commit both docs/ and snapshots/ironxl.json
```

`snapshots/ironxl.json` is committed, so a clone with no key still publishes a complete
comparison. The trade-off is explicit rather than hidden: those numbers came from different
hardware at a different time, the page says so, and the snapshot goes stale until someone with
a key re-runs it. Dependabot bumping `IronXL.Excel` will not refresh it — that needs a keyed run.

#### What the report scenario found

With a key, IronXL runs the whole scenario end to end and produces a complete workbook. Three
defects came out of validating the result and diffing its XML against the other five (the
artifact was not opened in Excel, so whether Excel offers to repair it is untested):

- **Conditional-format fills are silently discarded.** `PatternFormatting.BackgroundColor` is a
  hex string that IronXL converts to a `short` legacy-palette index before handing it to the
  NPOI model underneath. Every colour tried — pastel or primary, `#RRGGBB` or `#AARRGGBB` —
  emits `<bgColor indexed="0"/>`, i.e. black. The property getter reads the string back out of
  IronXL's own cache rather than the model, so it round-trips convincingly while the workbook
  receives none of it. The rule's **font** colour does apply, so the artifact ends up with
  green/red text on a black fill. There is no other public API for a conditional-format fill.
- **The two colour properties disagree on format.** `FontFormatting.FontColor` writes the digits
  through verbatim, so a 6-digit value lands as `rgb="006100"` and fails schema validation
  (OOXML requires 4-byte ARGB) — the alpha byte is mandatory. `PatternFormatting.BackgroundColor`
  does the opposite and truncates to 6 digits, reading `#FFFF0000` (opaque red) back as
  `#ffff00` (yellow).
- **`Font.Bold = true` alone emits invalid XML.** Setting nothing but bold on the header row
  produces `<color indexed="8" rgb="000000"/>` — again a 6-digit `rgb` where the schema wants 8.
  An unstyled workbook is clean, so this is IronXL's own doing and no public API avoids it. It
  is the single validation error in `output/stock-report-ironxl.xlsx`, and the same class of
  problem as the NPOI chart-title issue above.

Everything else in the scenario works: 20 chart series with a title and a real legend
(`SetLegendPosition` is honoured — `Right` emits an empty `<c:legendPos/>` only because "r" is
the schema default), `AutoSizeColumn` fits the week-ending column to 12.6 with `bestFit="1"`,
and both `conditionalFormatting` blocks carry the correct `sqref` and relative formulas.

#### API notes

- **It is NPOI underneath.** `IronXL.dll` contains ILMerged `NPOI.XSSF.UserModel` types and its
  own XML docs cross-reference `NPOI.SS.Formula.Formula`. The artifact is 79 KB against NPOI's
  79 KB and ClosedXML's 45 KB. Expect NPOI-shaped behaviour and NPOI-shaped output quirks.
- **`WorkBook` is not `IDisposable`.** The only eager-model library here with no deterministic
  release of a loaded workbook — relevant at the 100,000 × 15 read size.
- **No `SaveAs(Stream)`.** `ToStream()` allocates and returns its own `MemoryStream`, so the
  whole workbook is buffered in memory whatever the real destination is. The write benchmark
  measures that buffer because there is no way not to.
- **`AddSeries(values)` throws for line charts** — `"You must choose categories range for Line
  chart type together with values"`. The two-argument overload is mandatory.
- **Heavy transitive footprint.** It pulls in `Grpc.Net.Client`, `Google.Protobuf`, `Polly` and
  `IronSoftware.System.Drawing` for licensing and telemetry. `IronXlLicense.Ensure()` calls
  `License.DisableAppAnalytics()` so the phone-home cannot fold network latency into timings.

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
  chart, and the OpenXML SDK estimates its column width instead of measuring text, so each
  does less work than a full run of the scenario.
- Auto-fit widths legitimately differ between libraries, because each measures text with its own
  font engine and padding rule: ClosedXML 12.71, XLibur 13.00, EPPlus 14.53, NPOI 11.16,
  IronXL 12.6, and the OpenXML SDK's character-count estimate 11.71. A like-for-like width was
  not forced — the point is what each library does when simply asked to fit the column.
- IronXL's numbers are **snapshots**, not fresh measurements, unless the run that produced the
  page had a licence key. They are marked ⧗ wherever they appear and carry the version and date
  they were captured; treat them as indicative against the rest of the table.
- IronXL's report timing covers the full scenario, but its conditional-format fill never reaches
  the file (see above). It still pays for building the rule, so the timing stays comparable —
  the artifact is what differs.
- Every library now runs on its latest stable release. XLibur was previously pinned to a
  prerelease because stable `0.106.0` could not produce a valid chart for the report scenario;
  `0.200.0` can, so that pin is gone and its read and write numbers come from stable too.

## Best-effort implementations

Each library is driven the way its own documentation and idiomatic samples suggest, and every
benchmark was written to give that library its best shot at the scenario — streaming where a
streaming API exists, bulk or idiomatic iteration over per-cell indexer access, the cheapest
correct way to express each feature. Where a library forced a workaround, it is documented
above rather than quietly folded into the timing.

None of that makes these implementations authoritative. Knowing one of these libraries well is
not the same as knowing all seven well, the fastest path through an API is not always the
documented one, and a maintainer or heavy user will often spot in seconds something that took a
wrong turn here. **If a benchmark is doing something clumsy, or missing an API that would
produce the same result with less time or fewer allocations, please open a PR.**

A change is easiest to accept when it:

- **Produces the same result.** Same cells, same styles, same chart, same validity — a faster
  number that quietly drops a feature moves the library in the capability matrix, not the
  timings.
- **Uses public API only.** No reflection, no internals, no vendored source. The benchmark
  should reflect what a consumer of the NuGet package can actually write.
- **Says which version it needs.** If the better API only exists in a newer release, note it —
  the pinned versions are deliberate (see XLibur above).
- **Comes with a re-run.** Regenerate `docs/` locally (`./scripts/run-benchmarks.ps1`) and commit
  it, since CI numbers are not authoritative. IronXL will replay from `snapshots/` unless you
  hold a key, which is fine.

Corrections to the prose count too - if a capability-matrix cell or one of the API notes is
wrong or out of date, an issue is enough; you don't have to write the code.

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
5. If the library needs credentials to run at all, add it to `SnapshotLibraries` in
   `Program.cs` and gate it in `BenchmarkConfig` the way IronXL is, so runs without those
   credentials replay `snapshots/<library>.json` instead of failing.

## Dependency updates

To check every referenced package against nuget.org and bump the outdated ones in one pass:

```pwsh
# Report only — what could move, and to which version.
./scripts/update-libraries.ps1

# Fetch them (via `dotnet add package`, never by hand-editing the csproj).
./scripts/update-libraries.ps1 -Apply

# Narrow the scope.
./scripts/update-libraries.ps1 -Apply -LibrariesOnly
./scripts/update-libraries.ps1 -Package XLibur.Bundle -Apply
```

The report separates the Excel libraries under test from tooling/pins, so it is obvious
whether a bump changes the published numbers. Stable releases only, except for a package
already pinned to a prerelease (where the prerelease channel stays in scope) or when
`-IncludePrerelease` is passed.

[Dependabot](.github/dependabot.yml) covers the same ground automatically, opening weekly
PRs for outdated NuGet packages and GitHub Actions. Minor/patch bumps are grouped into one
PR; majors get their own. `build.yml` validates each PR (build + a Dry smoke run).

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
  Data/LibrarySnapshot.cs        # persisted results for licence-gated libraries
  Libraries/EpPlusLicense.cs     # EPPlus non-commercial license declaration
  Libraries/IronXlLicense.cs     # IronXL commercial key (opt-in via XLBENCH_IRONXL_KEY)
  Benchmarks/Read/*              # one class per library
  Benchmarks/Write/*             # one class per library
docs/                            # GitHub Pages content (index.md + generated results.md)
snapshots/                       # committed results for libraries that need a licence key
scripts/run-benchmarks.ps1        # run the suite and publish docs/
scripts/update-libraries.ps1      # check nuget.org for newer library versions and fetch them
```
