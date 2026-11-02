# charts-app: the interactive charts page

Builds `docs/charts.html`, the interactive page linked from the docs site. It reads the same
`docs/results-data.js` that `scripts/run-benchmarks.ps1` publishes, and bakes it into **one
self-contained HTML file** (JavaScript, CSS and data inlined), so a rebuild changes exactly one file
in git and the page opens from disk as well as from GitHub Pages.

## What it shows

| View | |
| --- | --- |
| **Charts** (`#/`) | Bar panels per operation (speed or memory), the overall dot plot, a head-to-head comparison of any two libraries, and the time-vs-memory trade-off scatter |
| **Heat table** (`#/heat`) | Log-shaded results per operation |
| **Table** (`#/table`) | The raw numbers |

View state lives in the URL, so a view can be shared: the measure, the hidden libraries and the
head-to-head pair (for example `charts.html#/?measure=mem&hide=Telerik,IronXL&a=XLibur&b=NPOI`).

## Commands

Needs Node 22.12 or newer and [pnpm](https://pnpm.io) (the version is pinned in `package.json`).

```pwsh
cd tools/charts-app
pnpm install
pnpm dev          # dev server with hot reload, opens /charts.html
pnpm typecheck    # tsc --noEmit
pnpm build        # type-checks, then writes ../../docs/charts.html
```

`pnpm dev` and `pnpm build` both start by validating `docs/results-data.js` (`scripts/prepare-data.mjs`).
A malformed payload fails there with a message naming the problem, instead of producing a blank page.
Set `XLBENCH_DATA` to point at a different data file.

## In the pipeline

| Where | What |
| --- | --- |
| `.github/workflows/docs-charts.yml` | After a merge to `main` that changes `docs/results-data.js` or anything under `tools/charts-app/`, builds the page and commits `docs/charts.html` back. This is how results published by `scripts/run-benchmarks.ps1` reach the site. |
| `.github/workflows/build.yml` (`charts` job) | On every pull request: installs from the lockfile, type-checks, builds, and checks that the build wrote only `docs/charts.html`. |
| `.github/dependabot.yml` | Weekly updates for this folder's packages. |

Do not commit `docs/charts.html` by hand; the workflow owns it.

## How it is built

- **TypeScript, strict**, with `noUncheckedIndexedAccess`. The payload's shape is a type (`src/model.ts`).
- **Preact, written against the React API.** TanStack Router and Table are React libraries, so the code
  imports `react` and the bundle aliases it to `preact/compat` (`vite.config.ts`). The real `react`
  packages are dev dependencies only because TanStack lists them as peers and TypeScript needs their
  types; they are never bundled.
- **Tailwind CSS 4**, with the page's design tokens as CSS variables (`src/index.css`). Dark mode follows
  `data-theme` on `<html>`.
- **TanStack Router** for the three views and URL state, with hash history so a static host needs no
  rewrite rules. **TanStack Table** renders the raw data table.
- **Data is baked in at build time**, not fetched, so there is no loading state and nothing to host
  besides the one file.
- **vite-plugin-singlefile** inlines everything. `build.emptyOutDir` is `false` because `docs/` holds the
  rest of the site; the `charts` CI job checks nothing else is touched.

## Layout

```text
charts.html          Vite entry (becomes docs/charts.html)
scripts/prepare-data.mjs   validates docs/results-data.js, writes src/generated/results.json (git-ignored)
src/main.tsx         shell: header, library toggles, routes
src/model.ts         payload types, the derived model, number formatting
src/views.tsx        bar panels, heat table, data table
src/dotplot.tsx      overall dot plot (ranked by geometric mean)
src/h2h.tsx          head-to-head
src/tradeoff.tsx     time-vs-memory scatter
src/tooltip.tsx      the one shared tooltip
src/ctx.ts           app context (model, URL state, shown libraries)
```

## Adding or changing a chart

Charts read `useApp()` (`src/ctx.ts`) for the model, the URL state and the libraries currently shown.
Use `useTip()` for hover detail so every chart shares one tooltip. Charts are data-driven: once the
publisher (`ResultsPublisher` in `src/XLBench/Program.cs`) writes a new scenario into `results-data.js`,
it appears here with no code change. The operation's title and tag come from the scenario's `label`
(`"Read · open + read all cells"`).
