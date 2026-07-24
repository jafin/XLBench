# ChartGen — interactive charts + heat table for the docs site

Generates `docs/charts.html`: an interactive, dependency-free page that renders the
benchmark results as sorted per-operation bar panels (with a Speed/Memory toggle), a
log-shaded **heat table**, a raw data table, and light/dark themes. It reads the exact
same `window.XLBENCH_DATA` payload that `scripts/run-benchmarks.ps1` already writes to
`docs/results-data.js`, so it plugs into the existing publish flow (Pages from `/docs`
on `main`).

## Pieces

| File | Role |
|------|------|
| `charts.template.html` | The page. **Single source of truth — edit here.** Renders entirely client-side; no CDN/build dependencies. |
| `Program.cs`, `ChartGen.csproj` | Tiny .NET tool that bakes `results-data.js` into a self-contained `docs/charts.html`. |
| `../../.github/workflows/docs-charts.yml` | CI: regenerates `docs/charts.html` when `results-data.js` changes and commits it back. |

The template has a placeholder — `const PAYLOAD = (/*__XLBENCH_DATA__*/ null) || window.XLBENCH_DATA` —
so it works **two ways**:

- **Committed as-is:** the placeholder stays `null` and the page loads `results-data.js`
  at runtime (there's a `<script src="results-data.js">` in the file). No build step.
- **Baked by ChartGen:** the placeholder is replaced with the JSON inline, producing a
  self-contained `docs/charts.html` with no runtime fetch. This is what CI commits.

Both paths render identically. Pick one; you don't need both.

## Run it locally

From the repo root:

```bash
dotnet run --project tools/ChartGen
# → writes docs/charts.html from docs/results-data.js + charts.template.html
```

Override paths if needed: `dotnet run --project tools/ChartGen -- <data.js> <template.html> <out.html>`.

You can also wire it into `scripts/run-benchmarks.ps1` so a local benchmark run refreshes
the page too — add after the `dotnet run` that produces the results:

```powershell
dotnet run --project tools/ChartGen -c Release
```

## CI (recommended — matches your "commit results, publish from /docs" flow)

`docs-charts.yml` triggers on pushes to `main` that touch `docs/results-data.js` (or the
tool/template), runs the generator, and commits the regenerated `docs/charts.html` back to
`main` with `[skip ci]`. No benchmarks run in CI, so it's fast and the numbers stay exactly
what you committed. GitHub Pages then serves the updated page from `/docs`.

Because it commits to `main`, the workflow needs `permissions: contents: write` (already set
in the file) and repository setting **Settings → Actions → General → Workflow permissions →
Read and write permissions**.

## Error whiskers (optional but recommended)

The bar panels can draw the BenchmarkDotNet **Error** (99.9% CI half-width) as a faint
whisker, and the tooltip shows Std. dev. Those fields aren't in the current
`results-data.js`, so the patch to `src/XLBench/Program.cs` (in this changeset) adds
`errorMs` and `stdDevMs` to the payload:

```csharp
var stats = report.ResultStatistics;
double? errorMs  = stats is not null ? stats.ConfidenceInterval.Margin / 1_000_000.0 : null;
double? stdDevMs = stats is not null ? stats.StandardDeviation      / 1_000_000.0 : null;
```

The page degrades gracefully: with no `errorMs` present it simply omits the whiskers and the
std-dev line, and hides the whisker sentence in the hint. Re-run
`scripts/run-benchmarks.ps1` after the patch to emit the richer payload.

## Notes

- **No YAML front matter** on the template on purpose — Jekyll copies front-matter-less
  files verbatim, so this full HTML document is served as-is (not wrapped in the Cayman
  layout), exactly like the original `docs/charts.html`.
- **Colors** are assigned per library from the `versions` order in the payload, so each
  library keeps one color across every panel. The palette carries 8 slots (validated for
  colorblind separation); a 9th library would cycle — fold rare libraries together or extend
  the `--s*` variables if you ever exceed 8.
- **The outlier** (e.g. ClosedXML reading every cell) is handled by scaling each panel
  independently rather than a global axis, so the smaller bars stay readable. The heat table
  uses a per-operation log scale for the same reason.
