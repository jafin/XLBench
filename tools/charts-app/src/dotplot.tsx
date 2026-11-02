// The overall dot plot.
// One row per library, one mark per operation: each value divided by the best value for that operation
// (best = 1x), on a log axis. Rows are ranked by the geometric mean of those ratios; a library that is
// missing any operation is not ranked, because a mean over a subset skips exactly the work it cannot do.
import { useLayoutEffect, useRef, useState } from 'react'
import type { RefObject } from 'react'
import { useApp } from './ctx'
import { fmtX, isNum, labelOf, type Model, type Op } from './model'
import { TipHead, TipLine, TipSub, useTip } from './tooltip'

type DotMeasure = 'time' | 'mem'

// circle, square, triangle up, diamond, triangle down, plus
const SHAPES = [
  'M0,-5A5,5 0 1,1 0,5A5,5 0 1,1 0,-5Z',
  'M-4.5,-4.5H4.5V4.5H-4.5Z',
  'M0,-6L5.4,3.6H-5.4Z',
  'M0,-6.2L6.2,0L0,6.2L-6.2,0Z',
  'M0,6L5.4,-3.6H-5.4Z',
  'M-1.9,-6H1.9V-1.9H6V1.9H1.9V6H-1.9V1.9H-6V-1.9H-1.9Z',
] as const

const fmtExact = (unit: string) => (v: number) =>
  `${v.toLocaleString(undefined, { maximumFractionDigits: v < 10 ? 2 : v < 100 ? 1 : 0 })} ${unit}`

const MEASURES: Record<DotMeasure, {
  field: 'time' | 'mem'; fmt: (v: number) => string; best: string; axis: string; title: string; button: string; sub: React.ReactNode; source: string
}> = {
  time: {
    field: 'time', fmt: fmtExact('ms'), best: 'fastest', axis: 'Relative time (× fastest)', button: 'Speed',
    title: 'Overall speed: time relative to the fastest, per operation',
    sub: <>Each row is one library; each mark is one operation’s mean time divided by the fastest time any library completed for that operation (fastest = 1×). <b>Lower is faster.</b> Rows are ranked by the geometric mean of the ratios. Hover a row for exact times.</>,
    source: 'mean times in the data (milliseconds)',
  },
  mem: {
    field: 'mem', fmt: fmtExact('MB'), best: 'leanest', axis: 'Relative memory (× leanest)', button: 'Memory',
    title: 'Overall memory: allocations relative to the leanest, per operation',
    sub: <>Each row is one library; each mark is one operation’s allocated memory divided by the least any library allocated for that operation (leanest = 1×). <b>Lower is leaner.</b> Rows are ranked by the geometric mean of the ratios. Hover a row for exact figures.</>,
    source: 'allocated megabytes in the data',
  },
}

interface DotOp extends Op { color: string; shape: string; best: number }
interface Pt { op: DotOp; v: number | null; ratio: number | null }
interface DotRow { lib: string; pts: Pt[]; done: number; gm: number; complete: boolean; rank?: number }
interface DotModel { ops: DotOp[]; ranked: DotRow[]; partial: DotRow[] }

function dotModel(model: Model, libs: string[], m: DotMeasure): DotModel {
  const f = MEASURES[m].field
  // Colour and shape belong to the operation's position, so they do not change when the measure does.
  const ops: DotOp[] = model.ops.flatMap((op, i) => {
    const vs = libs.map((l) => model.byOp[op.key]?.[l]?.[f]).filter(isNum)
    return vs.length ? [{ ...op, color: `var(--op${(i % 6) + 1})`, shape: SHAPES[i % SHAPES.length]!, best: Math.min(...vs) }] : []
  })
  const rows: DotRow[] = libs.flatMap((lib) => {
    const pts: Pt[] = ops.map((op) => {
      const v = model.byOp[op.key]?.[lib]?.[f]
      return isNum(v) ? { op, v, ratio: v / op.best } : { op, v: null, ratio: null }
    })
    const done = pts.filter((p): p is Pt & { ratio: number } => p.ratio != null)
    if (!done.length) return []
    const gm = Math.exp(done.reduce((s, p) => s + Math.log(p.ratio), 0) / done.length)
    return [{ lib, pts, done: done.length, gm, complete: done.length === ops.length }]
  })
  const ranked = rows.filter((r) => r.complete).sort((a, b) => a.gm - b.gm)
  ranked.forEach((r, i) => { r.rank = i + 1 })
  const partial = rows.filter((r) => !r.complete).sort((a, b) => b.done - a.done || a.gm - b.gm)
  return { ops, ranked, partial }
}

export function useWidth<T extends HTMLElement>(): [RefObject<T | null>, number] {
  const ref = useRef<T>(null)
  const [w, setW] = useState(0)
  useLayoutEffect(() => {
    const el = ref.current
    if (!el) return
    setW(el.clientWidth)
    const ro = new ResizeObserver((entries) => setW(entries[0]?.contentRect.width ?? el.clientWidth))
    ro.observe(el)
    return () => ro.disconnect()
  }, [])
  return [ref, w]
}

const Shape = ({ op, size = 14 }: { op: DotOp; size?: number }) => (
  <svg width={size} height={size} viewBox="-7 -7 14 14" aria-hidden="true" className="flex-none overflow-visible"><path d={op.shape} style={{ fill: op.color }} /></svg>
)

export function DotPlot() {
  const { model, active } = useApp()
  const [m, setM] = useState<DotMeasure>('time')
  const models = { time: dotModel(model, active, 'time'), mem: dotModel(model, active, 'mem') }
  // If the chosen measure has no data among the shown libraries, fall back to the other one.
  const cur: DotMeasure = models[m].ops.length ? m : (m === 'time' ? 'mem' : 'time')
  const M = MEASURES[cur], D = models[cur]
  if (!D.ops.length) return null
  const n = D.ops.length

  return (
    <section className="mt-7 rounded-2xl border border-line bg-surface p-4" aria-labelledby="dotTitle">
      <div className="flex flex-wrap items-center justify-between gap-x-4 gap-y-2">
        <h2 id="dotTitle" className="text-[15px] font-semibold tracking-tight">{M.title}</h2>
        <div className="inline-flex rounded-[9px] border border-line bg-track p-[3px]" role="group" aria-label="Overall chart measure">
          {(['time', 'mem'] as const).map((k) => (
            <button key={k} type="button" aria-pressed={cur === k} disabled={!models[k].ops.length} onClick={() => setM(k)}
              className="rounded-[7px] px-3.5 py-1.5 text-[13px] font-medium text-ink2 disabled:opacity-40 aria-pressed:bg-surface aria-pressed:text-ink aria-pressed:shadow-sm">{MEASURES[k].button}</button>))}
        </div>
      </div>
      <p className="mb-3 mt-1 max-w-[100ch] text-[13px] text-ink2">{M.sub}</p>
      <div className="mb-2 flex flex-wrap gap-x-4 gap-y-1.5">
        {D.ops.map((op) => (
          <span key={op.key} className="inline-flex items-center gap-[7px] text-[13px] text-ink2"><Shape op={op} />{op.tag ? `${op.tag} · ` : ''}{op.title}</span>))}
      </div>
      <Plot model={model} D={D} measure={cur} />
      <p className="mt-3.5 border-t border-grid pt-3 text-[11.5px] text-muted">
        Ratios are computed from the {M.source}, not from rounded figures. {cur === 'mem' && 'Memory is total managed bytes allocated over the run, not peak working set. '}
        {D.partial.length > 0 && <><b>Incomplete</b> libraries did not complete every one of the {n} operations, so they are not ranked: a mean over only the operations they finished would skip the work they cannot do. Their marks are still shown, and they are listed below the ranking with a partial geometric mean for reference only.</>}
      </p>
      <details className="mt-2.5 text-[13px]">
        <summary className="cursor-pointer text-[12.5px] text-ink2">Show as a table</summary>
        <div className="overflow-x-auto"><RatioTable model={model} D={D} measure={cur} /></div>
      </details>
    </section>
  )
}

function Plot({ model, D, measure }: { model: Model; D: DotModel; measure: DotMeasure }) {
  const tip = useTip()
  const [ref, width] = useWidth<HTMLDivElement>()
  const M = MEASURES[measure]
  const { ranked, partial } = D

  const W = Math.max(width, 300), narrow = W < 560
  const labelW = narrow ? 112 : 176, padR = 18, rowH = 36, top = 6, gapH = 34, axisH = 46
  const x0 = labelW + 12, x1 = W - padR
  const maxR = Math.max(1, ...[...ranked, ...partial].flatMap((r) => r.pts.map((p) => p.ratio ?? 1)))

  // 1-2-5 tick ladder up to the first tick at or above the largest ratio.
  const ladder: number[] = []
  for (let e = 0; ladder.length === 0 || ladder[ladder.length - 1]! < maxR; e++) [1, 2, 5].forEach((k) => ladder.push(k * 10 ** e))
  while (ladder.length > 1 && ladder[ladder.length - 2]! >= maxR) ladder.pop()
  const xMax = ladder[ladder.length - 1]!
  const xs = (v: number) => x0 + (Math.log(v) / Math.log(xMax)) * (x1 - x0)
  const minGap = narrow ? 34 : 30
  let ticks = ladder.filter((t, i, a) => i === 0 || xs(t) - xs(a[i - 1]!) >= minGap)
  if (ticks.length < ladder.length && ladder.some((t, i) => i > 0 && xs(t) - xs(ladder[i - 1]!) < minGap))
    ticks = ladder.filter((t) => Math.log10(t) % 1 === 0 || t === xMax)

  const plotH = (ranked.length + partial.length) * rowH + (partial.length ? gapH : 0)
  const H = top + plotH + axisH
  const DODGE = [0, -7, 7, -13, 13, -19]

  const rowTip = (r: DotRow) => (
    <>
      <TipHead>{labelOf(model, r.lib)}</TipHead>
      <TipSub>{r.complete ? `Rank ${r.rank} of ${ranked.length} · geo. mean ${fmtX(r.gm)}` : `Incomplete · ${r.done} of ${D.ops.length} operations · not ranked`}</TipSub>
      {r.pts.map((p) => (
        <TipLine key={p.op.key} k={<span className="inline-flex items-center gap-1.5"><Shape op={p.op} size={12} />{p.op.title}</span>}>
          {p.ratio == null ? <span className="text-muted">no result</span> : `${M.fmt(p.v!)} · ${fmtX(p.ratio)}${p.ratio < 1.005 ? ` (${M.best})` : ''}`}
        </TipLine>))}
    </>
  )

  let y = top
  const renderRow = (r: DotRow) => {
    const cy = y + rowH / 2
    y += rowH
    let run = 0, lastX = -1e9
    const marks = r.pts.filter((p): p is Pt & { ratio: number } => p.ratio != null).sort((a, b) => a.ratio - b.ratio).map((p) => {
      const x = xs(p.ratio)
      run = x - lastX < 10 ? run + 1 : 0    // marks closer than a mark's width step off the row line
      lastX = x
      return <path key={p.op.key} d={p.op.shape} transform={`translate(${x.toFixed(1)},${cy + DODGE[run % DODGE.length]!})`}
        style={{ fill: p.op.color, stroke: 'var(--surface)' }} strokeWidth={1.5} paintOrder="stroke" />
    })
    const tx = narrow ? 0 : 4
    return (
      <g key={r.lib} className="group" onMouseMove={(e) => tip.show(e, rowTip(r))} onMouseLeave={tip.hide}>
        <rect className="fill-transparent group-hover:fill-track" x={0} y={y - rowH + 1} width={W} height={rowH - 2} rx={6} />
        <line x1={x0} x2={x1} y1={cy} y2={cy} style={{ stroke: 'var(--grid)' }} strokeDasharray="2 3" />
        <text x={tx} y={cy - 3} fontSize={12.5} fontWeight={550} style={{ fill: r.complete ? 'var(--text-primary)' : 'var(--text-secondary)' }}>
          {!narrow && <tspan style={{ fill: 'var(--muted)' }}>{r.complete ? `${r.rank}. ` : ' '}</tspan>}{labelOf(model, r.lib)}
        </text>
        <text x={tx} y={cy + 11} fontSize={11} style={{ fill: 'var(--muted)' }}>
          {r.complete ? `${narrow ? `#${r.rank} · ` : ''}geo. mean ${fmtX(r.gm)}` : `${r.done}/${D.ops.length} ops · (${fmtX(r.gm)})`}
        </text>
        {marks}
      </g>
    )
  }

  const rankedRows = ranked.map(renderRow)
  let partialBlock: React.ReactNode = null
  if (partial.length) {
    const ly = y + gapH / 2 + 4
    const sepY = y + 8
    y += gapH
    partialBlock = (
      <>
        <line x1={0} x2={W} y1={sepY} y2={sepY} style={{ stroke: 'var(--baseline)' }} />
        <text x={narrow ? 0 : 4} y={ly + 4} fontSize={11} fontWeight={650} letterSpacing=".06em" style={{ fill: 'var(--muted)' }}>INCOMPLETE — NOT RANKED</text>
        {partial.map(renderRow)}
      </>
    )
  }

  return (
    <div ref={ref} className="relative w-full">
      {width > 0 && (
        <svg viewBox={`0 0 ${W} ${H}`} height={H} className="block w-full overflow-visible" role="img"
          aria-label={`Dot plot of ${M.axis.toLowerCase()} per operation for each library, log scale. Lower is better.`}>
          {ticks.map((t) => (
            <g key={t}>
              <line x1={xs(t)} x2={xs(t)} y1={top} y2={top + plotH} style={{ stroke: t === 1 ? 'var(--baseline)' : 'var(--grid)' }} strokeWidth={t === 1 ? 1.5 : 1} />
              <text x={xs(t)} y={top + plotH + 16} textAnchor="middle" fontSize={11.5} style={{ fill: 'var(--muted)' }}>{`${t.toLocaleString()}×`}</text>
            </g>))}
          <text x={(x0 + x1) / 2} y={top + plotH + 38} textAnchor="middle" fontSize={12.5} fontWeight={600} style={{ fill: 'var(--text-secondary)' }}>{M.axis}</text>
          {rankedRows}
          {partialBlock}
        </svg>
      )}
    </div>
  )
}

function RatioTable({ model, D, measure }: { model: Model; D: DotModel; measure: DotMeasure }) {
  const M = MEASURES[measure]
  const rows = [...D.ranked, ...D.partial]
  return (
    <table className="mt-2.5 w-full text-[13px]">
      <caption className="mb-2.5 text-left text-xs text-muted">
        {measure === 'time' ? 'Mean time (ms) and multiple of the fastest time' : 'Allocated memory (MB) and multiple of the leanest allocation'} for each operation. Geo. mean is the geometric mean of the multiples; incomplete libraries are not ranked.
      </caption>
      <thead><tr className="text-ink2">
        <th className="border-b border-baseline px-3 py-2 text-right font-semibold">Rank</th>
        <th className="border-b border-baseline px-3 py-2 text-left font-semibold">Library</th>
        {D.ops.map((op) => <th key={op.key} className="whitespace-nowrap border-b border-baseline px-3 py-2 text-right font-semibold">{op.title}</th>)}
        <th className="border-b border-baseline px-3 py-2 text-right font-semibold">Geo. mean</th>
      </tr></thead>
      <tbody>{rows.map((r) => (
        <tr key={r.lib} className="hover:bg-track">
          <td className="border-b border-grid px-3 py-2 text-right tabular-nums">{r.complete ? r.rank : 'Incomplete'}</td>
          <td className="border-b border-grid px-3 py-2 text-left">{labelOf(model, r.lib)}</td>
          {r.pts.map((p) => <td key={p.op.key} className="whitespace-nowrap border-b border-grid px-3 py-2 text-right tabular-nums">{p.ratio == null ? '—' : `${M.fmt(p.v!)} · ${fmtX(p.ratio)}`}</td>)}
          <td className="whitespace-nowrap border-b border-grid px-3 py-2 text-right tabular-nums">{r.complete ? fmtX(r.gm) : `(${fmtX(r.gm)}, ${r.done}/${D.ops.length})`}</td>
        </tr>))}
      </tbody>
    </table>
  )
}
