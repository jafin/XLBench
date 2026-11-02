// The bar panels (small multiples), the heat table and the raw data table.
import { useMemo, useState } from 'react'
import { createColumnHelper, tableFeatures, useTable } from '@tanstack/react-table'
import { useApp } from './ctx'
import { fmtMB, fmtMs, fmtVal, fmtX, isNum, labelOf, type Cell, type Measure, type Op } from './model'
import { TipHead, TipLine, TipSub, useTip } from './tooltip'

const valOf = (m: Measure, c: Cell) => (m === 'time' ? c.time : c.mem)

export function Swatch({ color }: { color: string }) {
  return <i className="mr-1.5 inline-block size-2.5 rounded-[3px] align-baseline shadow-[0_0_0_1px_var(--line)_inset]" style={{ background: color }} />
}

/** Small multiples: one panel per operation, bars sorted best to worst, each on its own scale. */
export function BarPanels() {
  const { model, search, active } = useApp()
  const tip = useTip()
  const m = search.measure
  const [hover, setHover] = useState<string | null>(null)   // "op|lib": every other row dims, as on the current page
  const rowProps = (key: string, content: () => React.ReactNode) => ({
    onMouseEnter: () => setHover(key),
    onMouseMove: (e: React.MouseEvent) => tip.show(e, content()),
    onMouseLeave: () => { setHover(null); tip.hide() },
  })
  const dim = (key: string) => (hover !== null && hover !== key ? 'opacity-[.28]' : '')
  return (
    <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3">
      {model.ops.map((op) => {
        const cells = model.byOp[op.key] ?? {}
        const rows = active.map((l) => cells[l]).filter((c): c is Cell => !!c && isNum(valOf(m, c)))
          .sort((a, b) => valOf(m, a)! - valOf(m, b)!)
        if (!rows.length) return null
        const max = Math.max(...rows.map((c) => valOf(m, c)!))
        const best = valOf(m, rows[0]!)!
        const missing = active.filter((l) => !rows.some((c) => c.lib === l))
        return (
          <section key={op.key} className="rounded-2xl border border-line bg-surface p-4">
            <div className="text-[11.5px] font-semibold uppercase tracking-wider text-muted">{op.tag}</div>
            <h2 className="text-sm font-semibold">{op.title}</h2>
            <p className="mb-3 mt-2 text-xs text-ink2">
              {m === 'time' ? 'Fastest' : 'Leanest'} here: <b className="text-ink">{labelOf(model, rows[0]!.lib)}</b> · {fmtVal(m, best)}
            </p>
            <div className="flex flex-col gap-3">
              {rows.map((c) => {
                const v = valOf(m, c)!
                const delta = (v / best - 1) * 100
                const pct = Math.max((v / max) * 100, 2.2)
                const err = m === 'time' && isNum(c.err) ? c.err : null
                return (
                  <div key={c.lib} className={dim(`${op.key}|${c.lib}`)} {...rowProps(`${op.key}|${c.lib}`, () => <CellTip model={model} cell={c} op={op} />)}>
                    <div className="mb-1 flex justify-between text-xs">
                      <span className="font-medium">{labelOf(model, c.lib)}</span>
                      <span className="tabular-nums text-ink2">{fmtVal(m, v)}{delta >= 0.05 && <span className="ml-1 text-muted">(+{delta >= 10 ? Math.round(delta) : delta.toFixed(1)}%)</span>}</span>
                    </div>
                    <div className="relative h-3 rounded bg-track">
                      <div className="absolute inset-y-0 left-0 min-w-[3px] rounded transition-[width] duration-500" style={{ width: `${pct}%`, background: model.colors[c.lib] }} />
                      {err != null && <span className="absolute top-1/2 h-[7px] -translate-y-1/2 border-x-[1.5px] border-muted opacity-60"
                        style={{ left: `${Math.max(((v - err) / max) * 100, 0)}%`, width: `${Math.min(((v + err) / max) * 100, 100) - Math.max(((v - err) / max) * 100, 0)}%` }} />}
                    </div>
                  </div>
                )
              })}
              {missing.map((l) => (
                <div key={l} className={dim(`${op.key}|${l}`)} {...rowProps(`${op.key}|${l}`, () => (
                  <><TipHead swatch={model.colors[l]}>{labelOf(model, l)}</TipHead><TipSub>{op.title}{op.tag ? ` · ${op.tag}` : ''}</TipSub>
                    <div className="text-ink2">No result: this library cannot perform this benchmark.</div></>))}>
                  <div className="mb-1 flex justify-between text-xs"><span className="font-medium text-ink2">{labelOf(model, l)}</span><span className="text-[10.5px] font-bold tracking-wider text-muted">BENCHMARK INCOMPLETE</span></div>
                  <div className="hatch h-3 rounded" />
                </div>
              ))}
            </div>
          </section>
        )
      })}
    </div>
  )
}

/** Cells shaded on a log scale per operation. Uses color-mix() instead of the old JS colour maths. */
export function HeatTable() {
  const { model, active } = useApp()
  return (
    <div className="overflow-x-auto rounded-2xl border border-line bg-surface p-4">
      <table className="w-full min-w-[560px] border-separate border-spacing-[3px] text-[13px]">
        <thead><tr className="text-xs text-ink2"><th className="px-3 pb-2 text-left">Library</th><th className="px-3 pb-2 text-right">Mean time</th><th className="px-1 pb-2 text-right">vs fastest</th><th className="px-3 pb-2 text-right">Allocated</th><th className="px-1 pb-2 text-right">vs leanest</th></tr></thead>
        {model.ops.map((op) => {
          const rows = active.map((l) => model.byOp[op.key]?.[l]).filter((c): c is Cell => !!c && isNum(c.time)).sort((a, b) => a.time! - b.time!)
          if (!rows.length) return null
          const tMin = rows[0]!.time!, tMax = rows[rows.length - 1]!.time!
          const mems = rows.map((c) => c.mem).filter(isNum), mMin = Math.min(...mems), mMax = Math.max(...mems)
          const heat = (v: number, lo: number, hi: number) => (hi > lo ? (Math.log(v) - Math.log(lo)) / (Math.log(hi) - Math.log(lo)) : 0)
          const cell = (v: number | null, lo: number, hi: number, text: string) => {
            if (!isNum(v)) return <td className="px-1 text-right text-muted">—</td>
            const t = heat(v, lo, hi)
            return <td className="rounded-md px-3 py-2 text-right tabular-nums shadow-[0_0_0_1px_var(--line)_inset]"
              style={{ background: `color-mix(in oklab, var(--heat-hot) ${(8 + 88 * t).toFixed(0)}%, var(--surface))`, color: t > 0.5 ? '#fff' : 'var(--text-primary)' }}>{text}</td>
          }
          return (
            <tbody key={op.key}>
              <tr><td colSpan={5} className="pb-1 pt-4 text-[11.5px] font-semibold uppercase tracking-wider text-muted"><b className="text-ink2">{op.tag}</b> · {op.title}</td></tr>
              {rows.map((c) => (
                <tr key={c.lib}>
                  <td className="px-1 font-medium"><Swatch color={model.colors[c.lib]!} />{labelOf(model, c.lib)}</td>
                  {cell(c.time, tMin, tMax, fmtMs(c.time!))}
                  <td className="px-1 text-right text-xs text-muted">{fmtX(c.time! / tMin)}</td>
                  {cell(c.mem, mMin, mMax, isNum(c.mem) ? fmtMB(c.mem) : '—')}
                  <td className="px-1 text-right text-xs text-muted">{isNum(c.mem) ? fmtX(c.mem / mMin) : '—'}</td>
                </tr>
              ))}
            </tbody>
          )
        })}
      </table>
    </div>
  )
}

// Table v9: features, columns and the table's type live at module scope so they stay referentially stable.
const features = tableFeatures({})
const helper = createColumnHelper<typeof features, Cell>()

/** Raw numbers through TanStack Table. */
export function DataTable() {
  const { model, active } = useApp()
  const data = useMemo(() => model.cells.filter((c) => active.includes(c.lib)), [model, active])
  const columns = useMemo(() => helper.columns([
    helper.accessor('lib', { header: 'Library', cell: (i) => labelOf(model, i.getValue()) }),
    helper.accessor('op', { header: 'Operation', cell: (i) => model.ops.find((o) => o.key === i.getValue())?.title ?? i.getValue() }),
    helper.accessor('time', { header: 'Mean (ms)', cell: (i) => i.getValue()?.toFixed(2) ?? '—' }),
    helper.accessor('err', { header: 'Error (ms)', cell: (i) => i.getValue()?.toFixed(2) ?? '—' }),
    helper.accessor('mem', { header: 'Allocated (MB)', cell: (i) => i.getValue()?.toFixed(2) ?? '—' }),
  ]), [model])
  const table = useTable({ features, columns, data })
  return (
    <div className="overflow-x-auto rounded-2xl border border-line bg-surface p-4">
      <table className="w-full text-[13px]">
        <thead>{table.getHeaderGroups().map((g) => (
          <tr key={g.id}>{g.headers.map((h) => (
            <th key={h.id} className="whitespace-nowrap border-b border-baseline px-3 py-2 text-left font-semibold text-ink2">{h.isPlaceholder ? null : <table.FlexRender header={h} />}</th>))}</tr>))}
        </thead>
        <tbody>{table.getRowModel().rows.map((r) => (
          <tr key={r.id} className="hover:bg-track">{r.getAllCells().map((c) => (
            <td key={c.id} className="whitespace-nowrap border-b border-grid px-3 py-2 tabular-nums"><table.FlexRender cell={c} /></td>))}</tr>))}
        </tbody>
      </table>
    </div>
  )
}

/** What the current page's tooltip showed: mean, error (99.9% CI), standard deviation and allocation. */
function CellTip({ model, cell, op }: { model: import('./model').Model; cell: Cell; op: Op }) {
  return (
    <>
      <TipHead swatch={model.colors[cell.lib]}>{labelOf(model, cell.lib)}</TipHead>
      <TipSub>{op.title}{op.tag ? ` · ${op.tag}` : ''}</TipSub>
      <TipLine k="Mean time">{isNum(cell.time) ? `${cell.time.toLocaleString(undefined, { maximumFractionDigits: 0 })} ms` : '—'}</TipLine>
      {isNum(cell.err) && <TipLine k="± Error (99.9% CI)">{cell.err.toLocaleString(undefined, { maximumFractionDigits: 0 })} ms</TipLine>}
      {isNum(cell.sd) && <TipLine k="Std. dev.">{cell.sd.toLocaleString(undefined, { maximumFractionDigits: 1 })} ms</TipLine>}
      {isNum(cell.mem) && <TipLine k="Allocated">{fmtMB(cell.mem)}</TipLine>}
    </>
  )
}
