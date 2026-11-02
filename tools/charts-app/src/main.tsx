// The XLBench interactive charts page. Built by Vite into one self-contained docs/charts.html
// (see vite.config.ts); the benchmark data is baked in at build time from docs/results-data.js.
import { StrictMode, useEffect, useState } from 'react'
import { createRoot } from 'react-dom/client'
import {
  Link, Outlet, RouterProvider, createHashHistory, createRootRoute, createRoute, createRouter, useNavigate, useRouterState,
} from '@tanstack/react-router'
import './index.css'
import payload from './generated/results.json'
import { AppCtx, type Search } from './ctx'
import { SNAP_MARK, buildModel, isNum, labelOf, type Measure, type Model, type Payload } from './model'
import { BarPanels, DataTable, HeatTable, Swatch } from './views'
import { HeadToHead } from './h2h'
import { DotPlot } from './dotplot'
import { TradeOff } from './tradeoff'
import { TooltipProvider } from './tooltip'

// scripts/prepare-data.mjs has already checked the payload's shape, so the cast is the boundary.
const data = payload as unknown as Payload
const model: Model | null = data.scenarios.length ? buildModel(data) : null

// ---- routing: views are routes, view state is in the URL ----
// Only non-default values reach the URL, so a plain visit stays a plain URL.
type RawSearch = Partial<Search>
function parseSearch(s: Record<string, unknown>): RawSearch {
  const str = (v: unknown) => (typeof v === 'string' && v ? v : undefined)
  const raw: RawSearch = {}
  if (s['measure'] === 'mem') raw.measure = 'mem'
  const hide = str(s['hide']), a = str(s['a']), b = str(s['b'])
  if (hide) raw.hide = hide
  if (a) raw.a = a
  if (b) raw.b = b
  return raw
}

const prefersDark = () => matchMedia('(prefers-color-scheme: dark)').matches

function Shell() {
  const raw = rootRoute.useSearch()
  const search: Search = { measure: raw.measure ?? 'time', hide: raw.hide ?? '', a: raw.a ?? '', b: raw.b ?? '' }
  const navigate = useNavigate()
  const onCharts = useRouterState({ select: (s) => s.location.pathname === '/' })
  const [dark, setDark] = useState(prefersDark)
  useEffect(() => { document.documentElement.dataset['theme'] = dark ? 'dark' : 'light' }, [dark])

  if (!model) {
    return (
      <p className="mx-auto max-w-[1180px] px-6 py-12 text-sm text-muted">
        No results yet. Run <code className="rounded bg-track px-1.5 py-0.5">scripts/run-benchmarks.ps1</code> to generate <code className="rounded bg-track px-1.5 py-0.5">results-data.js</code>.
      </p>
    )
  }

  const hidden = new Set(search.hide.split(',').filter(Boolean))
  const active = model.libs.filter((l) => !hidden.has(l))
  // Changing view state (measure, hidden libraries, the head-to-head pair) must not scroll the page:
  // the router resets scroll to the top on every navigation unless told otherwise.
  const setSearch = (patch: Partial<Search>) =>
    void navigate({ to: '.', search: (prev) => ({ ...prev, ...patch }), resetScroll: false })
  const toggle = (lib: string) => {
    const next = new Set(hidden)
    if (next.has(lib)) next.delete(lib); else if (active.length > 1) next.add(lib)   // always keep one library shown
    setSearch({ hide: [...next].join(',') })
  }
  // The payload names which libraries are commercial; nothing is hard-coded here.
  const commercial = model.commercial.filter((l) => model.libs.includes(l))
  const commercialShown = commercial.filter((l) => !hidden.has(l)).length
  const toggleCommercial = () => {
    if (commercialShown > 0) {
      // Hide the group, but never leave nothing shown.
      if (!model.libs.some((l) => !hidden.has(l) && !commercial.includes(l))) return
      setSearch({ hide: [...new Set([...hidden, ...commercial])].join(',') })
    } else {
      setSearch({ hide: [...hidden].filter((l) => !commercial.includes(l)).join(',') })
    }
  }
  const seg = (m: Measure, text: string) => (
    <button type="button" aria-pressed={search.measure === m} onClick={() => setSearch({ measure: m })}
      className="rounded-[7px] px-3.5 py-1.5 text-[13px] font-medium text-ink2 aria-pressed:bg-surface aria-pressed:text-ink aria-pressed:shadow-sm">{text}</button>)
  const nav = 'rounded-lg border border-line bg-surface px-3 py-1.5 text-[13px] font-medium text-ink2 hover:text-ink [&.active]:text-ink [&.active]:border-baseline'
  const snapshots = Object.entries(model.snaps)
  const hasErr = model.cells.some((c) => isNum(c.err))

  return (
    <AppCtx.Provider value={{ model, search, active, setSearch }}>
      <TooltipProvider>
        <div className="mx-auto max-w-[1180px] px-6 pb-14 pt-8">
          <header className="mb-1.5 flex flex-wrap items-end justify-between gap-4">
            <div>
              <h1 className="text-[22px] font-semibold tracking-tight">XLBench — interactive charts</h1>
              <p className="mt-1.5 max-w-[74ch] text-sm text-ink2">
                Read/write performance and memory across .NET Excel libraries. Lower is better on both measures — shorter bars are faster / leaner.
                See the <a className="text-link hover:underline" href="results.html">full result tables</a> and the <a className="text-link hover:underline" href="./">project overview</a>.
              </p>
              <p className="mt-2 text-[12.5px] text-muted">Last run: {model.updated}</p>
              <p className="mt-2 text-[12.5px] text-muted">Versions: {model.libs.map((l) => `${l} ${model.versions[l]}`).join('  ·  ')}</p>
              {snapshots.length > 0 && (
                <p className="mt-2 text-[12.5px] text-muted">{SNAP_MARK} Carried over from an earlier run, not measured here: {snapshots.map(([k, v]) => `${k} (${v})`).join('  ·  ')}</p>)}
            </div>
            <div className="flex flex-wrap items-center gap-2">
              <div className="inline-flex rounded-[9px] border border-line bg-track p-[3px]" role="group" aria-label="Measure">{seg('time', 'Speed (ms)')}{seg('mem', 'Memory (MB)')}</div>
              <Link to="/" search={(p) => p} className={nav}>Charts</Link>
              <Link to="/heat" search={(p) => p} className={nav}>Heat table</Link>
              <Link to="/table" search={(p) => p} className={nav}>Table</Link>
              <button type="button" className={nav} onClick={() => setDark(!dark)}>{dark ? 'Light' : 'Dark'}</button>
            </div>
          </header>
          {onCharts && (
            <p className="mb-5 mt-3.5 text-[12.5px] text-muted">
              Bars are sorted best → worst within each operation, each panel on its own scale. {hasErr && <>The faint whisker marks the benchmark <b className="text-ink2">error</b> (99.9% CI). </>}
              Hover any bar for detail. Click a library below to hide or show it; every chart rescales to the libraries still shown.
            </p>)}
          <div className="mb-4 mt-3 flex flex-wrap items-center gap-1.5" role="group" aria-label="Libraries shown">
            <span className="text-[12.5px] text-muted">Libraries:</span>
            {model.libs.map((l) => (
              <button key={l} type="button" aria-pressed={!hidden.has(l)} onClick={() => toggle(l)}
                title={hidden.has(l) ? 'Click to show' : active.length <= 1 ? 'At least one library must stay shown' : 'Click to hide'}
                className="rounded-full border border-line bg-surface px-3 py-1 text-[13px] font-medium aria-[pressed=false]:bg-transparent aria-[pressed=false]:text-muted aria-[pressed=false]:line-through">
                <Swatch color={model.colors[l]!} />{labelOf(model, l)}
              </button>))}
            {commercial.length > 0 && (
              <button type="button" onClick={toggleCommercial} title={`Show or hide all commercial libraries: ${commercial.join(', ')}`}
                className="rounded px-1.5 py-1 text-[12.5px] text-link hover:underline">
                Commercial: <b>{commercialShown === 0 ? 'hidden' : commercialShown === commercial.length ? 'shown' : `${commercialShown}/${commercial.length} shown`}</b>
              </button>)}
            {hidden.size > 0 && (
              <button type="button" onClick={() => setSearch({ hide: '' })} className="rounded px-1.5 py-1 text-[12.5px] text-link hover:underline">Show all</button>)}
          </div>
          <Outlet />
        </div>
      </TooltipProvider>
    </AppCtx.Provider>
  )
}

/** The note under the bar panels, which differs by measure. */
function Footnote() {
  const measure = rootRoute.useSearch({ select: (s) => s.measure ?? 'time' })
  return (
    <p className="mt-3.5 border-t border-grid pt-3 text-[11.5px] text-muted">
      {measure === 'time'
        ? 'Note: each panel is scaled independently, so a single very slow run (e.g. ClosedXML reading every cell) doesn’t crush the smaller bars. A library marked BENCHMARK INCOMPLETE cannot perform that operation (e.g. a streaming API has no workbook to edit), so it has no result and is always listed last.'
        : 'Note: memory is total managed bytes allocated over the run (not peak working set). A library marked BENCHMARK INCOMPLETE cannot perform that operation, so it has no result and is always listed last.'}
    </p>
  )
}

const rootRoute = createRootRoute({ component: Shell, validateSearch: parseSearch })
const chartsRoute = createRoute({ getParentRoute: () => rootRoute, path: '/', component: () => (<><BarPanels /><Footnote /><DotPlot /><HeadToHead /><TradeOff /></>) })
const heatRoute = createRoute({ getParentRoute: () => rootRoute, path: '/heat', component: HeatTable })
const tableRoute = createRoute({ getParentRoute: () => rootRoute, path: '/table', component: DataTable })

// Hash history: a static host (GitHub Pages) needs no rewrite rules, and the page also opens from file://.
const router = createRouter({
  routeTree: rootRoute.addChildren([chartsRoute, heatRoute, tableRoute]),
  history: createHashHistory(),
})
declare module '@tanstack/react-router' { interface Register { router: typeof router } }

document.documentElement.dataset['theme'] = prefersDark() ? 'dark' : 'light'   // before first paint, so no flash
createRoot(document.getElementById('root')!).render(
  <StrictMode><RouterProvider router={router} /></StrictMode>,
)
