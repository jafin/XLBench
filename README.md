# XLBench

Independent **performance and memory** benchmarks for .NET Excel libraries, consumed via
NuGet and run on **.NET 10** with [BenchmarkDotNet](https://benchmarkdotnet.org/). Results
are published as GitHub-flavored markdown to **GitHub Pages**.

📊 **Results:** https://jafin.github.io/XLBench/

## Libraries under test

| Library | NuGet package | Version | Notes | License |
| --- | --- | --- | --- | --- |
| [ClosedXML](https://github.com/ClosedXML/ClosedXML) | [`ClosedXML`](https://www.nuget.org/packages/ClosedXML) | 0.105.1 | High-level cell model | [MIT](https://licenses.nuget.org/MIT) |
| [EPPlus](https://github.com/EPPlusSoftware/EPPlus) | [`EPPlus`](https://www.nuget.org/packages/EPPlus) | 8.6.3 | Requires a license declaration (non-commercial, set in code) | [Polyform Noncommercial 1.0.0](https://polyformproject.org/licenses/noncommercial/1.0.0) ᴬ |
| [OpenXML SDK](https://github.com/dotnet/Open-XML-SDK) | [`DocumentFormat.OpenXml`](https://www.nuget.org/packages/DocumentFormat.OpenXml) | 3.5.1 | Low-level SAX streaming | [MIT](https://licenses.nuget.org/MIT) |
| [NPOI](https://github.com/nissl-lab/npoi) | [`NPOI`](https://www.nuget.org/packages/NPOI) | 2.8.0 | Java POI port | [Apache-2.0](https://licenses.nuget.org/Apache-2.0) ᴮ |
| [MiniExcel](https://github.com/mini-software/MiniExcel) | [`MiniExcel`](https://www.nuget.org/packages/MiniExcel) | 1.45.0 | Streaming, POCO/dynamic oriented | [Apache-2.0](https://licenses.nuget.org/Apache-2.0) |
| [XLibur](https://github.com/XLibur/XLibur) | [`XLibur.Bundle`](https://www.nuget.org/packages/XLibur.Bundle) | 0.300.0 | Bundles the SkiaSharp font engine (auto-registers) | [MIT](https://licenses.nuget.org/MIT) |
| [IronXL](https://ironsoftware.com/csharp/excel/) | [`IronXL.Excel`](https://www.nuget.org/packages/IronXL.Excel) | 2026.8.1 | **Commercial.** Runs only with a licence key; otherwise its results are replayed from `snapshots/` — see [IronXL](#ironxl--licence-gated-and-snapshotted) | [Proprietary EULA](https://ironsoftware.com/csharp/excel/licensing/) |

Library links point at each project's source repository, except IronXL, which is closed source —
that one goes to the product page. Versions are the pinned NuGet package versions (see
`src/XLBench/XLBench.csproj`); the generated `docs/results.md` reports the exact resolved
versions from each run via reflection.

Licenses are as declared by each pinned package (`<license>` in its `.nuspec`, or the license
file it ships), not as reported by any third-party index. Four are plain permissive terms; the
other three are not, and the distinction matters more than the benchmark numbers if you are
choosing a library for commercial work:

- **ᴬ EPPlus — noncommercial by default.** From 5.0 the free terms are Polyform Noncommercial,
  which permits use only for noncommercial purposes; commercial use needs a paid licence from
  EPPlus Software. That is what `Libraries/EpPlusLicense.cs` declares, and it is a licence
  choice this benchmark repository is entitled to make — it is not a template for your product.
- **ᴮ NPOI — Apache-2.0 code, fee-bearing binaries.** The source is Apache-2.0, but the NuGet
  binary release ships an Open Source Maintenance Fee EULA on top: the fee applies to users who
  use it in revenue-generating activity with annual gross revenue at or above US$10,000, and
  those below are exempt. Accepting it is what `<AcceptNPOIOSMFLicense>` in the project file
  does. Building from source under Apache-2.0 alone avoids the agreement entirely.
- **IronXL** is proprietary throughout — there is no free tier, only a trial. See
  [IronXL](#ironxl--licence-gated-and-snapshotted).

This is a summary for orientation, read off the packages this repository pins. It is not legal
advice, and it can go stale the moment a library relicenses — check the terms yourself before
depending on any of them.

## Scenarios

**Read** (50,000 × 15 sheet — every library reads the *same* `.xlsx` bytes):

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

**Edit** (`EditAndRecalculate` — mutate an existing workbook, then get the formulas right):

- *Init, not measured.* A canonical `.xlsx` is built once from `src/XLBench/Data/numbers.csv`
  (the first 500 of its 4,000 rows × 20 numeric columns — see below for why): a header row,
  then each data row carrying a `SUM(A:T)` row total in column U, with every second sheet row
  bold. Every library opens these exact bytes.
- *Measured.* Open the workbook, **delete every third data row** (sheet rows 4, 7, 10 … — 166
  of the 500, leaving 334), **set column A to `20`** on every surviving data row, then
  **recalculate** so each row's `SUM` reflects both edits. The result read back is the last
  surviving row's total, which `dotnet run -- edit` checks against the value computed straight
  from the CSV — every library must agree to the last ulp.
- Saving the edited workbook is *not* measured: serialization is what `CreateAndSave` already
  covers, and folding it in here would bury the difference this scenario exists to show.

The interesting cost is the delete. Removing a row means shifting every row beneath it up *and*
rewriting their `SUM` ranges, so a run of non-contiguous deletes is quadratic in every library
that maintains a cell model — and the constant factor varies by more than an order of magnitude
between them. It also separates the libraries that own a calculation engine from the one that
does not. Each library is handed the deletion set through the widest API it offers: a whole-set
call where there is one, a bottom-up loop where there is not.

**Insert** (`InsertColumnsAndRecalculate` — the same workbook, widened instead of shortened):

- *Init, not measured.* The **same source workbook** as the edit scenario, byte for byte, so the
  two are directly comparable and only the operation differs.
- *Measured.* Open the workbook, **insert 2 columns before column B**, write `10` into both of
  them on all 500 data rows, then **recalculate**. Nothing is deleted and no existing value is
  overwritten, so every row total must come back as its original total plus `20`.
- The columns go *inside* the totalled range, not at its edge, and that is the whole point:
  `SUM(A2:T2)` has to come back as `SUM(A2:V2)` and pick the new columns up. Inserting at column
  A would shift the range rather than widen it, which tests nothing.
- Saving is *not* measured, for the same reason as the edit scenario.

Where the delete makes each library re-point every formula 166 times, this is one structural edit
whose reference fixup runs across the workbook exactly once — so read the two together: the edit
scenario is dominated by per-formula cost repeated, this one by the cost of a single pass plus the
1,000 cells written into it. `dotnet run -- insert` checks it harder than the edit scenario checks
itself: not just the one cell the benchmark reads back, but **all 500 row totals in the saved
file**, read straight out of the package XML with no library in the way (see
[Reviewable output](#reviewable-output)).

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

### Edit scenario — capability matrix

| Library | Open + edit in place | Delete row (shift + reference fixup) | Calculation engine | Artifact valid? |
| --- | :-: | :-: | :-: | --- |
| ClosedXML | ✅ | ✅ `IXLRows.Delete()` | ✅ | ✅ |
| EPPlus | ✅ | ✅ `DeleteRow()` | ✅ | ✅ |
| OpenXML SDK | ⚠️ manual | ⚠️ manual | ❌ computed by the benchmark | ✅ |
| NPOI | ✅ | ⚠️ `RemoveRow` + `ShiftRows` | ✅ | ✅ |
| MiniExcel | ❌ | ❌ | ❌ | — not benchmarked |
| XLibur | ✅ | ✅ `IXLRows.Delete()` (batched) | ✅ | ✅ |
| IronXL | ✅ | ✅ `RemoveRow()` | ✅ | ✅ |

Unlike the report scenario, every benchmarked library here does the *same* work and produces the
same 335-row sheet: `A` = 20 on every data row, every `SUM(A{r}:T{r})` renumbered to its new row,
the bold-row pattern carried up with the shift, and totals that agree to the last ulp. The
timings are therefore directly comparable — with the OpenXML SDK the one caveat noted below.
`dotnet run -- edit` re-checks all of that against the CSV on every pass.

- **ClosedXML and XLibur — same call, different engine.** Both take the whole deletion set as
  `ws.Rows("4,7,10,…").Delete()`, so the two benchmarks are the same line of code. ClosedXML
  groups the set by sheet and then deletes bottom-up one row at a time, which is what the
  benchmark used to spell out by hand; XLibur (since `0.300.0`) collapses the set into a single
  row-deletion map, re-points every formula against it in one pass, and only then removes the
  rows run by run with the per-run formula pass switched off. Since the formula pass visits every
  formula in the workbook, doing it once instead of 166 times is most of the delete cost. It falls
  back to the per-run path for workbooks holding array, data-table or dynamic-array formulas,
  whose stored ranges the composite pass does not relocate — this one holds none.
- **NPOI — no delete-and-close-gap call.** `ISheet.RemoveRow` only empties the slot; closing it
  takes a separate `ShiftRows` over everything below, which is also what rewrites the shifted
  rows' `SUM` ranges. Two calls per deleted row, each touching every row beneath it. That is the
  finding rather than an artifact of how the benchmark is written — no bulk API exists to use
  instead.
- **OpenXML SDK — everything by hand, and no recalculation at all.** There is no `DeleteRow`:
  rows are `Remove()`d from `SheetData` and every surviving row below then has its `r` attribute,
  each of its cells' `CellReference`, and its `SUM` range renumbered explicitly. There is also no
  calculation engine, so the benchmark adds each row up itself and writes the result into the
  formula cell's cached `<v>`, drops the now-stale `calcChain` part, and sets `fullCalcOnLoad` so
  Excel rebuilds both on open. Read its timing against that: it is the only library here doing a
  single ordered pass with no model to maintain, and the only one whose "recalculation" is
  arithmetic the benchmark wrote.
- **MiniExcel — excluded.** It is a streaming reader/writer with no cell model, so there is
  nothing to open and mutate in place, and no formula support to recalculate. It could not run
  any part of this scenario.
- **Why 500 rows and not the full 4,000 in `numbers.csv`.** The quadratic delete cost is real: at
  the full file NPOI takes ~178 s *per operation*, which is roughly 80 minutes of measured run for
  this scenario alone. 500 rows keeps the quadratic shape and the ranking between libraries while
  bringing the whole scenario down to a couple of minutes. The row count is a single constant —
  `EditData.MaxRows` — so a deeper run is a one-line change.

### Insert scenario — capability matrix

| Library | Open + edit in place | Insert columns (shift + reference fixup) | Calculation engine | Totals persisted on save |
| --- | :-: | :-: | :-: | :-: |
| ClosedXML | ✅ | ✅ `IXLColumn.InsertColumnsBefore()` | ✅ | ⚠️ opt-in |
| EPPlus | ✅ | ✅ `InsertColumn(from, count)` | ✅ | ✅ |
| OpenXML SDK | ⚠️ manual | ⚠️ manual | ❌ computed by the benchmark | ✅ |
| NPOI | ✅ | ✅ `XSSFSheet.ShiftColumns()` | ✅ | ✅ |
| MiniExcel | ❌ | ❌ | ❌ | — not benchmarked |
| XLibur | ✅ | ✅ `IXLColumn.InsertColumnsBefore()` | ✅ | ✅ |
| IronXL | ✅ | ✅ `InsertColumns(index, count)` | ✅ | ✅ |

All six libraries do the *same* work here and produce the same 501-row × 23-column sheet: `B` and
`C` = 10 on every data row, every `SUM(A{r}:T{r})` widened to `SUM(A{r}:V{r})`, and totals that
agree to the last ulp. `dotnet run -- insert` re-checks all 500 of them against the CSV on every
pass, reading the saved file rather than asking the library that wrote it.

- **The last column is the point of the fourth column above.** A benchmark that recalculates in
  memory and saves a file with no cached values has done the arithmetic but not shipped it: the
  numbers only exist once Excel opens the workbook and computes them again. Four libraries write
  their totals into the file as a matter of course. ClosedXML does not — a plain `SaveAs` emits
  each formula cell as `<f>` with no `<v>` — so the artifact save opts in with
  `evaluateFormulae: true`. That is outside the measured region and changes no timing; without it
  the check reports 500 cells with no usable cached value, which is how the difference was found.
  The same is true of ClosedXML's *edit* artifact, which this repo has never checked.
- **NPOI — an insert spelled as a shift.** There is no "insert n columns here"; the equivalent is
  `XSSFSheet.ShiftColumns`, moving the block that has to make way. It is a real structural call
  and not a workaround — internally it builds a `FormulaShifter` for the column move and re-points
  formulas, named ranges, conditional formatting and hyperlinks against it — but it lives on the
  concrete `XSSFSheet` rather than on `ISheet`, where its row counterpart `ShiftRows` sits. Note
  the contrast with the same library's row delete, which has no bulk call at all: here one call
  covers the whole edit.
- **OpenXML SDK — everything by hand, and no recalculation at all.** There is no `InsertColumn`.
  Every cell from column B rightwards has its `CellReference` rewritten two columns along, the two
  new cells are spliced into the gap *in column order* — `sheetData` is an ordered element list,
  so appending them would produce a file Excel rejects — and each row's `SUM` range is widened
  explicitly. As in the edit scenario there is no calculation engine, so the benchmark adds each
  row up itself, writes the result into the cached `<v>`, drops the now-stale `calcChain` (whose
  entries still point at the old column) and sets `fullCalcOnLoad`.
- **Styling of the inserted columns is library-defined and deliberately not equalized.** Some
  libraries carry a neighbouring column's style onto the new ones, the hand-written OpenXML path
  leaves them unstyled, so the bold-row pattern may or may not extend across B and C. Forcing
  agreement would mean adding per-library styling code to a measured region to paper over a real
  difference in what each library's insert does.
- **MiniExcel — excluded**, for the same reason as the edit scenario.

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
  release of a loaded workbook — relevant at the 50,000 × 15 read size.
- **No `SaveAs(Stream)`.** `ToStream()` allocates and returns its own `MemoryStream`, so the
  whole workbook is buffered in memory whatever the real destination is. The write benchmark
  measures that buffer because there is no way not to.
- **`AddSeries(values)` throws for line charts** — `"You must choose categories range for Line
  chart type together with values"`. The two-argument overload is mandatory.
- **Heavy transitive footprint.** It pulls in `Grpc.Net.Client`, `Google.Protobuf`, `Polly` and
  `IronSoftware.System.Drawing` for licensing and telemetry. `IronXlLicense.Ensure()` calls
  `License.DisableAppAnalytics()` so the phone-home cannot fold network latency into timings.

### Reviewable output

The report, edit and insert benchmarks keep their workbooks. Each run writes
`output/stock-report-<library>.xlsx`, `output/numbers-edited-<library>.xlsx` and
`output/numbers-inserted-<library>.xlsx` (git-ignored, overwritten on the next run) so the results
can be opened and eyeballed:

```pwsh
# Just write the artifacts — same code path, no measurement (seconds, not minutes).
dotnet run -c Release --project src/XLBench -- report
dotnet run -c Release --project src/XLBench -- edit
dotnet run -c Release --project src/XLBench -- insert
```

`-- edit` additionally verifies each library's recalculated row total against the value computed
straight from `numbers.csv` and prints `OK` or `MISMATCH` per library, so the scenario's
correctness can be checked without a measured run. The elapsed time it prints is one cold pass —
indicative only, never a benchmark result.

`-- insert` goes further, and deliberately: the benchmark reads one cell back, which is enough to
stop the work being optimized away but not enough to call the result correct. So it re-opens each
saved artifact and checks **every** row — all 500 totals against the CSV, and every `SUM` range
against the widened form it should have taken. It reads them straight out of the package XML
(`Data/SavedSheet.cs`) rather than through a library, because checking a library's output by
re-opening it with a library risks the reader evaluating the formula and reporting the answer the
check wanted rather than the one stored in the file. What it prints per library is
`500/500 saved row totals verified`, or the first few cells that disagree with the reason.

Saving happens in `[GlobalCleanup]`, once per library and outside every measured iteration, by
re-running the build against a `FileStream` — so file I/O never lands in the timings. If a
workbook is still open in Excel the save is skipped with a warning rather than failing the run.

### Fairness notes

- OpenXML SDK and MiniExcel are streaming APIs with no eager "load workbook" step, so they
  only appear in `OpenAndReadAll`.
- MiniExcel has no formula engine; its write total is a pre-computed value, not a `SUM()`.
- MiniExcel cannot open and mutate a workbook in place, so it does not appear in
  `EditAndRecalculate` or `InsertColumnsAndRecalculate`.
- The shared read file, and the source workbook the edit and insert scenarios share, are generated
  once with ClosedXML purely as a neutral OOXML producer, outside any measured region.
- Read timings and allocations both scale linearly with the sheet, which is 50,000 × 15
  (750,000 cells). That size is deliberate: it is well past the point where the fixed cost of
  opening the package matters, and still heavy enough to push the eager-model libraries into
  multi-gigabyte allocation — which is the finding the allocation column is there to carry. A
  smaller sheet keeps the ranking but flattens that into per-cell overhead.
  `TestData.ReadRowCount` is one constant if you want a deeper run.
- Edit timings are comparable across all six libraries — they produce identical sheets — with
  the OpenXML SDK the one caveat: it has no calculation engine, so its "recalculation" is a sum
  the benchmark computes and writes into the cached value itself. The same caveat, and the same
  comparability, applies to the insert scenario.
- The insert scenario's artifact save is the one place a library gets a flag the others do not:
  ClosedXML saves with `evaluateFormulae: true` so the file carries its totals, which the other
  libraries do without being asked. It is outside the measured region and affects no timing.
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
# Full suite (writes docs/results.md), with the tuned warmup/iteration counts below.
./scripts/run-benchmarks.ps1

# Subset
./scripts/run-benchmarks.ps1 -Filter '*Write*'
./scripts/run-benchmarks.ps1 -Filter '*Read*' -Job short

# BenchmarkDotNet's stock defaults — slowest, most trustworthy. Use for published numbers.
./scripts/run-benchmarks.ps1 -FullFidelity
```

Or directly:

```pwsh
dotnet run -c Release --project src/XLBench -- --filter '*'
dotnet run -c Release --project src/XLBench -- --filter '*ClosedXml*' --job short
```

### Warmup and iteration counts

Every scenario here runs between 25 ms and 20 s per operation, so BenchmarkDotNet's stock
floors — a 6-iteration minimum warmup and a 15-iteration minimum workload — cost minutes per
benchmark buying accuracy this comparison does not need. The libraries differ by 2–10×, far
outside any plausible confidence interval. `run-benchmarks.ps1` therefore lowers the bounds on
both adaptive stages by default:

| Stage | BenchmarkDotNet default | XLBench default |
| --- | --- | --- |
| Warmup | 6 – 50 | 1 – 3 |
| Workload | 15 – 100 | 5 – 10 |

Both stages still stop early once measurements settle, so these move the floor and ceiling
rather than pinning a count: a stable benchmark finishes at the minimum, and only a noisy one
runs to the maximum. Measured on the edit scenario, this cut the wall clock from 2m34s to 1m05s
with means within ~5% of a full-fidelity run, the ranking unchanged, and Error columns 2–3×
wider.

That trade is right for comparing libraries and **wrong for detecting a few-percent regression
between versions of one library** — use `-FullFidelity` for the numbers you publish after a
dependency bump. Individual bounds are overridable (`-MinIterationCount`, `-MaxWarmupCount`, …),
and a `-Job` preset pins the counts outright and so takes precedence over all of them.

> BenchmarkDotNet validates that each maximum exceeds its minimum and fails the run otherwise,
> so lowering a maximum past the stock minimum means lowering the minimum in the same call.
> `--maxIterationCount 10` on its own is rejected, because the minimum is still 15.

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
   can express them) `Report/<Name>ReportBenchmarks.cs`, `Edit/<Name>EditBenchmarks.cs` and
   `Insert/<Name>InsertBenchmarks.cs`, mirroring an existing set. Method names must match —
   `OpenWorkbook` / `OpenAndReadAll` / `CreateAndSave` / `CreateStockReport` /
   `EditAndRecalculate` / `InsertColumnsAndRecalculate` — so the joined summary aligns.
3. Add a case to `LibraryNameColumn` in `src/XLBench/Config/LibraryComparisonConfig.cs`.
4. For a report, edit or insert benchmark, register it in `Data/ReportArtifacts.cs`,
   `Data/EditArtifacts.cs` or `Data/InsertArtifacts.cs` so `dotnet run -- report` / `-- edit` /
   `-- insert` writes its workbook, and add a row to the matching capability matrix above.
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
  Data/StockData.cs              # report-scenario dataset (embedded stock_data.json)
  Data/EditData.cs               # edit-scenario dataset + source workbook (embedded numbers.csv)
  Data/InsertData.cs             # insert-scenario layout over the same source workbook
  Data/SavedSheet.cs             # reads a saved .xlsx's cells from the package XML, no library
  Data/LibrarySnapshot.cs        # persisted results for licence-gated libraries
  Libraries/EpPlusLicense.cs     # EPPlus non-commercial license declaration
  Libraries/IronXlLicense.cs     # IronXL commercial key (opt-in via XLBENCH_IRONXL_KEY)
  Benchmarks/Read/*              # one class per library
  Benchmarks/Write/*             # one class per library
  Benchmarks/Report/*            # one class per library that can express the scenario
  Benchmarks/Edit/*              # one class per library that can express the scenario
  Benchmarks/Insert/*            # one class per library that can express the scenario
docs/                            # GitHub Pages content (index.md + generated results.md)
snapshots/                       # committed results for libraries that need a licence key
scripts/run-benchmarks.ps1        # run the suite and publish docs/
scripts/update-libraries.ps1      # check nuget.org for newer library versions and fetch them
```
