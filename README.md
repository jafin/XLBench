# XLBench

Independent **performance and memory benchmarks** for .NET Excel libraries.

Libraries are installed from NuGet and tested on **.NET 10** using [BenchmarkDotNet](https://benchmarkdotnet.org/). 

📊 **Results:** are publish here https://jafin.github.io/XLBench/

## Libraries under test

| Library                                                                                | NuGet package                                                                                                                                                                                                        | Version    | Notes                                                                                          | License                                                                                                |
| -------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------- | ---------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| [ClosedXML](https://github.com/ClosedXML/ClosedXML)                                    | [`ClosedXML`](https://www.nuget.org/packages/ClosedXML)                                                                                                                                                              | 0.105.1    | High-level cell model                                                                          | [MIT](https://licenses.nuget.org/MIT)                                                                  |
| [EPPlus](https://github.com/EPPlusSoftware/EPPlus)                                     | [`EPPlus`](https://www.nuget.org/packages/EPPlus)                                                                                                                                                                    | 8.7.0      | Requires a licence declaration                                                                 | [Polyform Noncommercial 1.0.0](https://polyformproject.org/licenses/noncommercial/1.0.0) ᴬ             |
| [OpenXML SDK](https://github.com/dotnet/Open-XML-SDK)                                  | [`DocumentFormat.OpenXml`](https://www.nuget.org/packages/DocumentFormat.OpenXml)                                                                                                                                    | 3.5.1      | Low-level OOXML API                                                                            | [MIT](https://licenses.nuget.org/MIT)                                                                  |
| [NPOI](https://github.com/nissl-lab/npoi)                                              | [`NPOI`](https://www.nuget.org/packages/NPOI)                                                                                                                                                                        | 2.8.0      | Java POI port                                                                                  | [Apache-2.0](https://licenses.nuget.org/Apache-2.0) ᴮ                                                  |
| [MiniExcel](https://github.com/mini-software/MiniExcel)                                | [`MiniExcel`](https://www.nuget.org/packages/MiniExcel)                                                                                                                                                              | 1.46.0     | Streaming, POCO/dynamic focused                                                                | [Apache-2.0](https://licenses.nuget.org/Apache-2.0)                                                    |
| [XLibur](https://github.com/XLibur/XLibur)                                             | [`XLibur.Bundle`](https://www.nuget.org/packages/XLibur.Bundle)                                                                                                                                                      | 0.610.0    | Includes and registers the SkiaSharp font engine                                               | [MIT](https://licenses.nuget.org/MIT)                                                                  |
| [IronXL](https://ironsoftware.com/csharp/excel/)                                       | [`IronXL.Excel`](https://www.nuget.org/packages/IronXL.Excel)                                                                                                                                                        | 2026.9.2   | **Commercial.** Requires a licence key. Without one, saved benchmark results are used instead. | [Proprietary EULA](https://ironsoftware.com/csharp/excel/licensing/)                                   |
| [Telerik](https://www.telerik.com/document-processing-libraries) (RadSpreadProcessing) | [`Telerik.Documents.Spreadsheet`](https://www.nuget.org/packages/Telerik.Documents.Spreadsheet) + [`.FormatProviders.OpenXml`](https://www.nuget.org/packages/Telerik.Documents.Spreadsheet.FormatProviders.OpenXml) | 2026.3.826 | **Commercial.** Unlicensed workbooks get a licence worksheet instead of failing.               | [Proprietary EULA](https://www.telerik.com/purchase/license-agreement/document-processing-libraries) ᶜ |

Library links point to source repositories where available. IronXL and Telerik are closed source, so their links go to product pages.

Versions are pinned in `src/XLBench/XLBench.csproj`. Generated results also show the exact package versions loaded at runtime.

Licences are taken from the pinned packages themselves, not from third-party package indexes.

Important licence notes:

* **ᴬ EPPlus:** free use is limited to non-commercial use under Polyform Noncommercial. Commercial use requires a paid licence.
* **ᴮ NPOI:** the source is Apache-2.0, but the NuGet package also includes an Open Source Maintenance Fee agreement. The fee applies to revenue-generating users with annual gross revenue of at least US$10,000. Building directly from the Apache-licensed source does not require accepting that extra agreement.
* **ᶜ Telerik:** commercial software with a 30-day trial. Without a valid licence it still runs, but exported workbooks contain an extra `License` worksheet.
* **IronXL:** proprietary commercial software with a trial and no permanent free tier.

This is only a summary of the terms shipped with the versions used by XLBench. Check the current licence before choosing a library for your own project.

## Scenarios

### Read

The main read workbook contains **50,000 rows × 15 columns**. Every library reads the same `.xlsx` bytes.

#### `OpenAmendPropertiesAndSave`

Tests a metadata round trip:

1. Open a workbook.
2. Change `Title` and `Category`.
3. Save it back to a stream.

This uses a smaller **1,000 × 8** workbook because the test is about opening, changing metadata and saving, not large-sheet throughput.

The returned result is the serialized workbook length so the save cannot be optimized away.

Streaming-only libraries do not take part in this test.

#### `OpenAndReadAll`

Opens the workbook and reads every populated cell as a string.

Each library uses its normal API:

* ClosedXML/XLibur: `CellsUsed()`
* EPPlus: `Cells`
* NPOI: row enumeration
* OpenXML SDK/MiniExcel: streaming

Random `Cell(row, col)` access is avoided where it is not the library's normal usage pattern.

### Write

#### `CreateAndSave`

Creates a 50,000-row workbook containing strings, numbers, dates and a final `SUM`.

Each library builds the workbook and saves it to a `MemoryStream`.

### Report

#### `CreateStockReport`

Creates a small workbook that tests Excel features rather than raw volume.

It loads `src/XLBench/Data/stock_data.json` containing:

* 20 stock symbols
* 260 weekly prices per symbol
* 5,200 price records

The result is a **261-row × 22-column** sheet with:

* `Week`
* `Week Ending`
* one price column per symbol

It then:

* adds conditional formatting for weekly price changes,
* auto-fits the week-ending column,
* adds a line chart for all 20 symbols.

This scenario has only 5,742 cells, so its timing mostly reflects the cost of styles, conditional formatting, chart APIs and text measurement rather than bulk cell throughput.

### Edit

#### `EditAndRecalculate`

Tests editing an existing workbook.

**Setup, not measured**

A shared workbook is built from the first 500 rows of `numbers.csv`:

* 20 numeric columns,
* a `SUM(A:T)` total in column U,
* every second sheet row in bold.

Every library opens the same workbook bytes.

**Measured**

Each library:

1. Opens the workbook.
2. Deletes every third data row.
3. Sets column A to `20` on the remaining rows.
4. Recalculates the formulas.

The benchmark reads the total from the last remaining row and checks it against the value calculated directly from the CSV.

Saving is not included in the benchmark because write performance is already covered by `CreateAndSave`.

Deleting rows is intentionally expensive. Each delete may require rows and formulas below it to be updated, so libraries that process each delete separately can become much slower.

### Insert

#### `InsertColumnsAndRecalculate`

Uses the same source workbook as the Edit scenario.

Each library:

1. Opens the workbook.
2. Inserts two columns before column B.
3. Writes `10` into both new columns for all 500 rows.
4. Recalculates.

The original formula:

`SUM(A2:T2)`

must become:

`SUM(A2:V2)`

so that the two new columns are included.

Saving is not measured.

This scenario complements Edit:

* Edit performs 166 separate row deletions.
* Insert performs one structural change across the workbook.

`dotnet run -- insert` also verifies all 500 totals in the saved workbook by reading the OOXML directly.

Every benchmark uses `[MemoryDiagnoser]`, so results include allocations and Gen0/1/2 collections as well as time.

## Report scenario — capability matrix

| Library     | Import + grid | Conditional formatting | Auto-fit column |   Chart   | Artifact valid?   |
| ----------- | :-----------: | :--------------------: | :-------------: | :-------: | ----------------- |
| ClosedXML   |       ✅       |            ✅           |        ✅        |     ❌     | ✅ schema-clean    |
| EPPlus      |       ✅       |            ✅           |        ✅        |     ✅     | ✅ schema-clean    |
| OpenXML SDK |       ✅       |        ⚠️ manual       |   ⚠️ estimated  | ⚠️ manual | ✅ schema-clean    |
| NPOI        |       ✅       |            ✅           |        ✅        |     ✅     | ⚠️ 1 schema error |
| MiniExcel   |       ✅       |            ❌           |        ❌        |     ❌     | — not benchmarked |
| XLibur      |       ✅       |            ✅           |        ✅        |     ✅     | ✅ schema-clean    |
| IronXL      |       ✅       |      ⚠️ font only      |        ✅        |     ✅     | ⚠️ 1 schema error |
| Telerik     |       ✅       |            ✅           |        ✅        |     ✅     | ✅ schema-clean    |

All generated charts include a legend on the right.

### ClosedXML

ClosedXML 0.105.x contains internal chart types, but there is no public worksheet API for adding charts.

Its report benchmark therefore includes data and conditional formatting, but no chart.

### OpenXML SDK

The SDK can create the full report, but most of it must be built manually:

* styles,
* differential formats,
* conditional formatting,
* drawing anchors,
* chart XML.

It also has no font measurement engine, so auto-fit is approximated from the longest string instead of measured.

### NPOI

NPOI can create the full chart, but `XDDFChart.SetTitleText` writes schema-invalid XML for the chart title. The workbook therefore has one validation error and Excel may offer to repair it.

Removing the title produces a clean workbook.

`XDDFLineChartData.SetGrouping` also throws on a chart created by NPOI, so the benchmark sets the grouping through the underlying CT model.

### MiniExcel

MiniExcel does not support conditional formatting or charts.

Benchmarking only its data-writing part against libraries doing the full report would be misleading, so it is excluded from this scenario.

### XLibur

XLibur charts do not show a legend unless `Legend.Visible` is enabled, so the benchmark sets it explicitly.

Older versions also produced invalid chart series XML, but those issues have since been fixed in stable releases.

### Telerik

Telerik requires less chart setup than most libraries. `ChartCollection.Add` can infer series and names from the selected range.

However, the category column must be assigned separately or Telerik treats it as another series.

Telerik charts are sized using pixels rather than a second cell anchor, so the benchmark converts the shared chart size into an equivalent pixel size.

## Edit scenario — capability matrix

| Library     | Open + edit |  Delete row + fix references |   Calculation engine   | Artifact valid?   |
| ----------- | :---------: | :--------------------------: | :--------------------: | ----------------- |
| ClosedXML   |      ✅      |     ✅ `IXLRows.Delete()`     |            ✅           | ✅                 |
| EPPlus      |      ✅      |        ✅ `DeleteRow()`       |            ✅           | ✅                 |
| OpenXML SDK |  ⚠️ manual  |           ⚠️ manual          | ❌ benchmark calculates | ✅                 |
| NPOI        |      ✅      | ⚠️ `RemoveRow` + `ShiftRows` |            ✅           | ✅                 |
| MiniExcel   |      ❌      |               ❌              |            ❌           | — not benchmarked |
| XLibur      |      ✅      |     ✅ `IXLRows.Delete()`     |            ✅           | ✅                 |
| IronXL      |      ✅      |        ✅ `RemoveRow()`       |            ✅           | ✅                 |
| Telerik     |      ✅      |   ✅ `RowSelection.Remove()`  |            ✅           | ✅                 |

All benchmarked libraries produce the same 335-row result:

* column A = 20,
* formulas updated to their new row numbers,
* bold formatting moved with the rows,
* totals matching the CSV.

`dotnet run -- edit` checks all of this.

### ClosedXML and XLibur

Both use:

`ws.Rows("4,7,10,…").Delete()`

ClosedXML processes the rows one by one internally.

XLibur builds a single deletion map, updates formulas once, then removes the rows. This avoids repeating a workbook-wide formula update for every deleted row.

### Telerik

Telerik can delete all 166 scattered rows in one call.

It produces the correct result, but the operation is much slower and allocates much more memory than the other libraries.

Undo history is enabled by default and adds significant overhead, so the benchmark disables it with:

`workbook.History.IsEnabled = false`

### NPOI

`ISheet.RemoveRow` only removes the row contents. It does not close the gap.

The benchmark must therefore call `ShiftRows` after each removal. This also updates formulas.

There is no bulk delete API.

### OpenXML SDK

The SDK has no row-delete API.

The benchmark manually:

* removes rows,
* renumbers remaining rows,
* updates cell references,
* updates formula ranges.

The SDK also has no calculation engine, so the benchmark calculates the totals itself, writes them into the cached values, removes the stale calculation chain and asks Excel to fully recalculate when opened.

### MiniExcel

MiniExcel is a streaming reader/writer. It has no editable workbook model or formula calculation engine, so it cannot run this scenario.

### Why only 500 rows?

Row deletion is roughly quadratic for several libraries.

Using all 4,000 source rows makes some tests extremely slow. NPOI, for example, takes around 178 seconds per operation.

500 rows keeps the same performance behaviour while making the benchmark practical to run.

The row count is controlled by `EditData.MaxRows`.

## Insert scenario — capability matrix

| Library     | Open + edit |   Insert columns + fix references   |   Calculation engine   |    Totals saved   |
| ----------- | :---------: | :---------------------------------: | :--------------------: | :---------------: |
| ClosedXML   |      ✅      | ✅ `IXLColumn.InsertColumnsBefore()` |            ✅           |     ⚠️ opt-in     |
| EPPlus      |      ✅      |    ✅ `InsertColumn(from, count)`    |            ✅           |         ✅         |
| OpenXML SDK |  ⚠️ manual  |              ⚠️ manual              | ❌ benchmark calculates |         ✅         |
| NPOI        |      ✅      |     ✅ `XSSFSheet.ShiftColumns()`    |            ✅           |         ✅         |
| MiniExcel   |      ❌      |                  ❌                  |            ❌           | — not benchmarked |
| XLibur      |      ✅      | ✅ `IXLColumn.InsertColumnsBefore()` |            ✅           |         ✅         |
| IronXL      |      ✅      |   ✅ `InsertColumns(index, count)`   |            ✅           |         ✅         |
| Telerik     |      ✅      |     ✅ `ColumnSelection.Insert()`    |            ✅           |         ✅         |

All seven libraries produce the same **501-row × 23-column** workbook:

* columns B and C = 10,
* `SUM(A:T)` becomes `SUM(A:V)`,
* totals match the CSV.

`dotnet run -- insert` verifies all 500 rows directly from the saved OOXML.

### Saved formula values

A library can recalculate a formula in memory without saving the result into the file.

ClosedXML normally saves the formula without its cached `<v>` value. For the generated artifact, XLBench therefore saves with:

`evaluateFormulae: true`

This happens outside the measured benchmark.

The other libraries write the cached total without an extra option.

### NPOI

NPOI has no direct "insert columns" API.

`XSSFSheet.ShiftColumns()` moves the existing columns and updates formulas, named ranges, conditional formatting and hyperlinks.

Unlike row deletion, this can be done in one structural operation.

### OpenXML SDK

Column insertion is manual.

The benchmark:

* moves all cell references from column B onwards,
* inserts the new cells in the correct XML order,
* updates formula ranges,
* recalculates totals itself,
* writes cached values,
* removes the stale calculation chain.

### Styling

Libraries differ in how inserted columns inherit styles.

XLBench does not force them to match because doing so would require extra library-specific code inside the measured operation.

### MiniExcel

MiniExcel cannot edit an existing workbook in place, so it is excluded.

## IronXL — licence-gated and snapshotted

IronXL is implemented for all benchmark scenarios, but only runs when `XLBENCH_IRONXL_KEY` contains a valid licence key.

Without a key, XLBench uses the committed results in:

`snapshots/ironxl.json`

This keeps IronXL visible in the comparison without requiring every contributor to own a licence.

### How snapshots work

A run **with** a valid key measures IronXL and updates `snapshots/ironxl.json`.

The snapshot stores:

* timings,
* allocations,
* BenchmarkDotNet table rows,
* IronXL version,
* machine information,
* job,
* capture time.

A run **without** a key skips IronXL and inserts the saved results into the generated tables and charts.

Snapshot values are marked **⧗**.

Partial runs update only the methods that were measured.

Two checks prevent an incompatible snapshot row from being used:

* the table columns must still match,
* the `Mean` unit must still match.

Chart values are stored separately in milliseconds.

```pwsh
# Refresh the IronXL snapshot.
$env:XLBENCH_IRONXL_KEY = 'IRONXL.YOURCOMPANY.IRO######.####'
./scripts/run-benchmarks.ps1
```

Afterwards, commit both `docs/` and `snapshots/ironxl.json`.

Snapshots may come from different hardware and may become stale after an IronXL version update. The generated page clearly marks them as snapshots.

### Report findings

IronXL can build the full report, but validation found several issues.

#### Conditional-format fills are lost

`PatternFormatting.BackgroundColor` converts the requested colour to a legacy palette index.

The resulting workbook writes:

`<bgColor indexed="0"/>`

instead of the requested colour.

The font colour is applied correctly, so the generated rules can end up with coloured text on a black fill.

#### Colour formats are inconsistent

`FontFormatting.FontColor` can write a six-digit RGB value where OOXML requires eight-digit ARGB.

`PatternFormatting.BackgroundColor` has the opposite problem and can truncate an eight-digit value.

#### Bold text can produce invalid colour XML

Setting `Font.Bold = true` can also produce a six-digit `rgb` attribute.

This is the single validation error in the IronXL report artifact.

Other report features work correctly, including:

* 20 chart series,
* chart title,
* legend,
* column auto-fit,
* conditional-format ranges and formulas.

### IronXL API notes

* IronXL includes NPOI types internally, so some behaviour and generated XML closely resemble NPOI.
* `WorkBook` is not `IDisposable`.
* There is no `SaveAs(Stream)`. `ToStream()` creates and returns its own `MemoryStream`.
* `AddSeries(values)` fails for line charts unless a category range is supplied.
* IronXL pulls in several large dependencies for licensing and telemetry.
* XLBench disables IronXL application analytics so network activity cannot affect benchmark timings.

## Telerik — licence-gated

Telerik RadSpreadProcessing is implemented for all five scenarios.

It is commercial software and behaves differently from IronXL when no licence is available.

### Unlicensed behaviour

Nothing throws.

The project builds and runs, but every exported workbook gets an extra worksheet named `License` containing a licence-validation message.

That would make Telerik's benchmark unfair because it would be writing an extra worksheet.

XLBench therefore performs a small export at startup and checks for that sheet. If it appears, Telerik benchmarks are skipped.

CI has no licence, so Telerik does not appear in `docs/results-ci.md`.

### Licence handling

Telerik resolves its licence at build time.

It checks:

* `TELERIK_LICENSE`
* `TELERIK_LICENSE_PATH`
* `telerik-license.txt`
* `%AppData%\Telerik\telerik-license.txt`

The licence is then embedded into the built assembly.

Changing the licence therefore requires rebuilding the project.

The licensing task also contacts Telerik during the build.

### Snapshots

Telerik results are not currently stored in `snapshots/`.

A licensed run includes Telerik. An unlicensed run simply leaves it out.

It could be added to the existing snapshot system later if required.

### Telerik API notes

* Rows and columns are **0-based**.
* `Workbook.DocumentInfo` has no `Category` property.
* File import/export is handled by `XlsxFormatProvider`, not by `Workbook`.
* The spreadsheet model and OpenXML format provider are separate packages.
* There is no direct equivalent of `CellsUsed()`. The benchmark loops through `UsedCellRange`, matching Telerik's own examples.
* Formula results are accessed through `FormulaCellValue.GetResultValueAsCellValue()`.
* Undo history is enabled by default and is expensive.

## Reviewable output

The report, edit and insert scenarios also save their workbooks:

* `output/stock-report-<library>.xlsx`
* `output/numbers-edited-<library>.xlsx`
* `output/numbers-inserted-<library>.xlsx`

These files are git-ignored and overwritten on the next run.

```pwsh
dotnet run -c Release --project src/XLBench -- report
dotnet run -c Release --project src/XLBench -- edit
dotnet run -c Release --project src/XLBench -- insert
```

These commands generate the workbooks without running BenchmarkDotNet.

### Edit validation

`dotnet run -- edit` checks each recalculated total against the CSV and prints `OK` or `MISMATCH`.

The displayed elapsed time is only a single cold run and is not a benchmark result.

### Insert validation

`dotnet run -- insert` checks all 500 saved row totals.

It also checks that every formula has been widened correctly.

The validation reads the raw OOXML using `Data/SavedSheet.cs` instead of reopening the workbook through the library that created it.

This avoids a library recalculating the formula while being used to validate its own output.

### Saving benchmark artifacts

Artifacts are saved during `[GlobalCleanup]`, outside the measured iterations.

This keeps file I/O out of the benchmark timings.

If an output workbook is already open in Excel, saving is skipped with a warning.

## Fairness notes

* OpenXML SDK and MiniExcel are streaming APIs, so they do not take part in the eager workbook-open benchmark.
* MiniExcel has no formula engine. Its write benchmark stores a pre-calculated total instead of a `SUM()` formula.
* MiniExcel cannot edit an existing workbook, so it is excluded from Edit and Insert.
* Shared input workbooks are generated once with ClosedXML outside the measured code.
* Telerik has no document `Category` property, so its metadata test only changes `Title`.
* ClosedXML, XLibur, NPOI and IronXL keep the existing OPC core-properties location when changing metadata.
* EPPlus writes a new `docProps/core.xml` while leaving the original core-properties relationship in the package. This can produce two core-properties relationships. Excel reads the workbook, but strict readers may choose the wrong one.
* The main read sheet contains 750,000 cells. This is large enough to expose memory behaviour while still being practical to benchmark.
* Edit and Insert produce matching output across libraries, except that OpenXML SDK performs formula calculation in benchmark code because the SDK has no calculation engine.
* ClosedXML uses `evaluateFormulae: true` only while saving the Insert artifact so cached totals are present in the file. This happens outside the measured benchmark.
* Report JSON is parsed once outside the benchmark.
* Report timings must be read together with the capability matrix. ClosedXML does not create a chart and OpenXML SDK does not perform real auto-fit.
* Auto-fit widths differ because each library uses a different text-measurement engine and padding rules.
* IronXL snapshot numbers may have been measured on another machine and are marked **⧗**.
* IronXL builds the conditional-format rule but fails to save the requested fill colour.
* Telerik's report benchmark has high variance because it allocates around 430 MB per operation and triggers Gen2 collections. Its result should be read as roughly **160 ms ±25 ms**, not as an exact value.
* Telerik results only appear when the run is licensed.
* XLBench uses the latest stable release of each library.

## Best-effort implementations

Each benchmark tries to use the library in the way its documentation recommends:

* streaming where available,
* bulk operations where available,
* normal iteration APIs instead of slow per-cell access,
* the cheapest public API that produces the required result.

Workarounds are documented instead of hidden.

These implementations are not guaranteed to be perfect. Library maintainers and experienced users may know faster or cleaner ways to perform the same operation.

If a benchmark can produce the same result with less time or memory, please open a PR.

A useful change should:

* **Produce the same result.** Do not remove features just to improve the timing.
* **Use public APIs only.** No reflection, internals or vendored source.
* **State the required version.** If the API only exists in a newer release, mention it.
* **Include regenerated results.** Run `./scripts/run-benchmarks.ps1` and commit `docs/`.

IronXL can continue using its existing snapshot if you do not have a licence key.

Documentation corrections are also welcome. If a capability or API note is wrong, an issue is enough.

## Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/download), pinned with `global.json`
* Linux only: `libfontconfig1` for XLibur's SkiaSharp font engine

## Running

```pwsh
# Full suite
./scripts/run-benchmarks.ps1

# Run part of the suite
./scripts/run-benchmarks.ps1 -Filter '*Write*'
./scripts/run-benchmarks.ps1 -Filter '*Read*' -Job short

# BenchmarkDotNet's normal higher-accuracy settings
./scripts/run-benchmarks.ps1 -FullFidelity
```

Or run BenchmarkDotNet directly:

```pwsh
dotnet run -c Release --project src/XLBench -- --filter '*'
dotnet run -c Release --project src/XLBench -- --filter '*ClosedXml*' --job short
```

### Warmup and iteration counts

Some XLBench operations take up to 20 seconds each.

BenchmarkDotNet's default minimums make a complete run much slower than needed for broad library comparisons.

XLBench therefore uses smaller defaults:

| Stage    | BenchmarkDotNet default | XLBench default |
| -------- | ----------------------- | --------------- |
| Warmup   | 6–50                    | 1–3             |
| Workload | 15–100                  | 5–10            |

The stages are still adaptive. Stable benchmarks stop early, while noisy benchmarks run for longer.

For the Edit scenario, these settings reduced total run time from about **2m34s to 1m05s** while keeping means within about 5% and preserving the same overall performance differences.

These faster settings are useful for comparing libraries.

They are **not** ideal for measuring small performance changes between two versions of the same library. Use:

```pwsh
./scripts/run-benchmarks.ps1 -FullFidelity
```

for published results after dependency changes.

Individual limits can also be changed with options such as:

* `-MinIterationCount`
* `-MaxIterationCount`
* `-MaxWarmupCount`

A `-Job` preset overrides these values.

BenchmarkDotNet requires each maximum to be greater than its matching minimum. For example, setting `--maxIterationCount 10` without also lowering the default minimum of 15 will fail validation.

The script publishes the cleaned results to:

`docs/results.md`

Raw BenchmarkDotNet output under `BenchmarkDotNet.Artifacts/` is git-ignored.

Commit and push `docs/` to update GitHub Pages.

## Publishing

GitHub Pages is served from:

**`main` → `/docs`**

Configure it under:

Settings → Pages → Deploy from a branch → `main` / `/docs`

CI machines can be noisy, so the benchmark numbers published on the site come from local runs that are committed to the repository.

CI only checks that the project builds and the benchmarks can run.

An optional full benchmark workflow is available in:

`.github/workflows/benchmark.yml`

## Adding a library

1. Add its NuGet `PackageReference` to `src/XLBench/XLBench.csproj`.
2. Add benchmark classes under `Read`, `Write` and any other scenarios the library supports.
3. Keep the benchmark method names the same so results line up:

   * `OpenAmendPropertiesAndSave`
   * `OpenAndReadAll`
   * `CreateAndSave`
   * `CreateStockReport`
   * `EditAndRecalculate`
   * `InsertColumnsAndRecalculate`
4. Add the library to `LibraryNameColumn` in `LibraryComparisonConfig.cs`.
5. Register any generated report/edit/insert artifacts.
6. Add the library to the relevant capability matrices.
7. If credentials are required, gate the benchmark and optionally add snapshot support like IronXL.

## Dependency updates

Check NuGet for newer versions:

```pwsh
# Show available updates
./scripts/update-libraries.ps1

# Apply updates
./scripts/update-libraries.ps1 -Apply

# Only benchmark libraries
./scripts/update-libraries.ps1 -Apply -LibrariesOnly

# One package
./scripts/update-libraries.ps1 -Package XLibur.Bundle -Apply
```

The script separates benchmark libraries from tooling dependencies so it is clear which updates may affect results.

Stable versions are used by default. Pre-release versions are only considered when:

* the package is already pinned to a pre-release version, or
* `-IncludePrerelease` is supplied.

[Dependabot](.github/dependabot.yml) also checks NuGet packages and GitHub Actions each week.

Minor and patch updates are grouped together. Major updates get separate PRs.

`build.yml` confirms that Dependabot updates build and pass a smoke run.

> Merging a dependency update does **not** refresh the published benchmark results.
>
> After updating a benchmarked library, run `scripts/run-benchmarks.ps1` again and commit the regenerated `docs/`.

## Project layout

```text
src/XLBench/
  Program.cs                     # command handling and results publishing
  Config/LibraryComparisonConfig # joined results, memory diagnostics, GitHub export
  Data/TestData.cs               # shared deterministic data
  Data/StockData.cs              # report data
  Data/EditData.cs               # edit data and source workbook
  Data/InsertData.cs             # insert scenario
  Data/SavedSheet.cs             # reads saved cells directly from OOXML
  Data/LibrarySnapshot.cs        # saved results for licence-gated libraries

  Libraries/EpPlusLicense.cs
  Libraries/IronXlLicense.cs
  Libraries/TelerikLicense.cs
  Libraries/TelerikWorkbooks.cs

  Benchmarks/Read/*
  Benchmarks/Write/*
  Benchmarks/Report/*
  Benchmarks/Edit/*
  Benchmarks/Insert/*

docs/                            # GitHub Pages content
snapshots/                       # saved results for licence-gated libraries
scripts/run-benchmarks.ps1       # run benchmarks and publish results
scripts/update-libraries.ps1     # check and update NuGet packages
```
