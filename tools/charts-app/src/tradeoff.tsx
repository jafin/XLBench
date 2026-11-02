// The time-vs-memory trade-off scatter.
// Every library is a point: x = time as a multiple of the fastest, y = memory as a multiple of the leanest,
// both log scale, so bottom-left is best on both. Click a library to trace its operations.
import { useState } from 'react'
import { useApp } from './ctx'
import { useWidth } from './dotplot'
import { fmtMB, fmtMs, fmtX, isNum, labelOf, type Model, type Op } from './model'
import { TipHead, TipLine, TipSub, useTip } from './tooltip'

type Scope = 'all' | string   // 'all' = geometric mean of every operation, otherwise one operation's key

interface OpPt { op: Op; x: number; y: number; t: number; m: number }
interface LibPt { lib: string; x: number; y: number; n: number; per: OpPt[]; full: boolean }

const geoMean = (a: number[]) => Math.exp(a.reduce((s, v) => s + Math.log(v), 0) / a.length)

function points(model: Model, libs: string[], scope: Scope): LibPt[] {
  // The best value per operation among the libraries shown, so ratios rescale when libraries are hidden.
  const best = (op: Op, f: 'time' | 'mem') => {
    const vs = libs.map((l) => model.byOp[op.key]?.[l]?.[f]).filter(isNum)
    return vs.length ? Math.min(...vs) : null
  }
  return libs.flatMap((lib): LibPt[] => {
    const per: OpPt[] = model.ops.flatMap((op) => {
      const c = model.byOp[op.key]?.[lib], bt = best(op, 'time'), bm = best(op, 'mem')
      return c && isNum(c.time) && isNum(c.mem) && bt && bm ? [{ op, x: c.time / bt, y: c.mem / bm, t: c.time, m: c.mem }] : []
    })
    if (scope === 'all') {
      return per.length ? [{ lib, x: geoMean(per.map((p) => p.x)), y: geoMean(per.map((p) => p.y)), n: per.length, per, full: per.length === model.ops.length }] : []
    }
    const p = per.find((q) => q.op.key === scope)
    return p ? [{ lib, x: p.x, y: p.y, n: 1, per: [p], full: true }] : []
  })
}

/** 1-2-5 tick values up to the first one at or above `max`. */
function ladder(max: number): number[] {
  const out: number[] = []
  for (let e = 0; ; e++) for (const k of [1, 2, 5]) { const t = k * 10 ** e; out.push(t); if (t >= max) return out }
}

export function TradeOff() {
  const { model, active } = useApp()
  const tip = useTip()
  const [scope, setScope] = useState<Scope>('all')
  const [trail, setTrail] = useState<string | null>(null)
  const [ref, width] = useWidth<HTMLDivElement>()

  const pts = points(model, active, scope)
  const trailPt = scope === 'all' ? pts.find((p) => p.lib === trail) : undefined

  const W = Math.max(width, 360), H = 520, L = 62, R = 120, T = 14, B = 54
  const all = [...pts, ...(trailPt?.per ?? [])]
  const xMax = Math.max(1.5, ...all.map((p) => p.x)) * 1.2, yMax = Math.max(1.5, ...all.map((p) => p.y)) * 1.2
  const xs = (v: number) => L + (Math.log(v) / Math.log(xMax)) * (W - L - R)
  const ys = (v: number) => H - B - (Math.log(v) / Math.log(yMax)) * (H - B - T)
  const xTicks = ladder(xMax).filter((t) => t <= xMax * 1.001), yTicks = ladder(yMax).filter((t) => t <= yMax * 1.001)

  // Libraries nobody beats on both axes (the Pareto frontier), left to right.
  const front = pts.filter((p) => !pts.some((q) => q !== p && q.x <= p.x && q.y <= p.y && (q.x < p.x || q.y < p.y))).sort((a, b) => a.x - b.x)

  // Label placement: each label tries right, left, above, then below, and takes the first spot that overlaps
  // no marker and no label already placed. (Checking each point on its own let two neighbours claim the same gap.)
  type Side = 'right' | 'left' | 'above' | 'below'
  type Rect = { x0: number; x1: number; y0: number; y1: number }
  const hit = (a: Rect, b: Rect) => a.x0 < b.x1 && a.x1 > b.x0 && a.y0 < b.y1 && a.y1 > b.y0
  const markerR = 10
  const markers: Rect[] = pts.map((p) => ({ x0: xs(p.x) - markerR - 1, x1: xs(p.x) + markerR + 1, y0: ys(p.y) - markerR - 1, y1: ys(p.y) + markerR + 1 }))
  const taken: Rect[] = []
  const sides = new Map<string, Side>()
  for (const p of [...pts].sort((a, b) => a.x - b.x)) {
    const w = labelOf(model, p.lib).length * 7.4, cx = xs(p.x), cy = ys(p.y), g = markerR + 4
    const spots: [Side, Rect][] = [
      ['right', { x0: cx + g, x1: cx + g + w, y0: cy - 9, y1: cy + 5 }],
      ['left', { x0: cx - g - w, x1: cx - g, y0: cy - 9, y1: cy + 5 }],
      ['above', { x0: cx - w / 2, x1: cx + w / 2, y0: cy - g - 14, y1: cy - g }],
      ['below', { x0: cx - w / 2, x1: cx + w / 2, y0: cy + g, y1: cy + g + 14 }],
    ]
    const own = markers[pts.indexOf(p)]
    const free = spots.find(([, r]) => !markers.some((m) => m !== own && hit(r, m)) && !taken.some((t) => hit(r, t)) && r.x0 > L - 50 && r.x1 < W)
    const [side, rect] = free ?? spots[0]!
    sides.set(p.lib, side)
    taken.push(rect)
  }

  const pointTip = (p: LibPt) => (
    <>
      <TipHead swatch={model.colors[p.lib]}>{labelOf(model, p.lib)}</TipHead>
      <TipSub>{scope === 'all' ? `Geo. mean over ${p.n} of ${model.ops.length} operations` : model.ops.find((o) => o.key === scope)?.title}</TipSub>
      <TipLine k="Time">{fmtX(p.x)} the fastest</TipLine>
      <TipLine k="Memory">{fmtX(p.y)} the leanest</TipLine>
      {scope !== 'all' && p.per[0] && <><TipLine k="Mean time">{fmtMs(p.per[0].t)}</TipLine><TipLine k="Allocated">{fmtMB(p.per[0].m)}</TipLine></>}
      {!p.full && <div className="mt-1 text-muted">Hollow: some operations are missing.</div>}
      <div className="mt-1 text-muted">{trail === p.lib ? 'Click to hide' : 'Click to trace'} its operations</div>
    </>
  )

  return (
    <section className="mt-7 rounded-2xl border border-line bg-surface p-4" aria-labelledby="tradeTitle">
      <h2 id="tradeTitle" className="text-[15px] font-semibold tracking-tight">Trade-off: time vs memory</h2>
      <p className="mb-3 mt-1 max-w-[80ch] text-[13px] text-ink2">Each library is a point. Bottom-left is best on both axes. Click a library to trace its operations.</p>
      <div className="mb-3 flex flex-wrap items-center gap-x-3 gap-y-2">
        <label className="text-[13px] text-ink2">Scope{' '}
          <select value={scope} onChange={(e) => { setScope(e.target.value); setTrail(null) }}
            className="ml-1 rounded-lg border border-line bg-surface px-2.5 py-1.5 text-[13px] font-medium text-ink">
            <option value="all">Overall (geometric mean of every operation)</option>
            {model.ops.map((o) => <option key={o.key} value={o.key}>{o.tag ? `${o.tag} · ` : ''}{o.title}</option>)}
          </select>
        </label>
      </div>
      <div ref={ref} className="w-full">
        {width > 0 && (
          <svg viewBox={`0 0 ${W} ${H}`} height={H} className="block w-full overflow-visible" role="img"
            aria-label="Scatter of memory against time for each library, both on log scales. Bottom-left is best.">
            {xTicks.map((t) => (
              <g key={`x${t}`}>
                <line x1={xs(t)} x2={xs(t)} y1={T} y2={H - B} style={{ stroke: t === 1 ? 'var(--baseline)' : 'var(--grid)' }} />
                <text x={xs(t)} y={H - B + 16} textAnchor="middle" fontSize={11.5} style={{ fill: 'var(--muted)' }}>{`${t.toLocaleString()}×`}</text>
              </g>))}
            {yTicks.map((t) => (
              <g key={`y${t}`}>
                <line x1={L} x2={W - R} y1={ys(t)} y2={ys(t)} style={{ stroke: t === 1 ? 'var(--baseline)' : 'var(--grid)' }} />
                <text x={L - 8} y={ys(t) + 4} textAnchor="end" fontSize={11.5} style={{ fill: 'var(--muted)' }}>{`${t.toLocaleString()}×`}</text>
              </g>))}
            <text x={(L + W - R) / 2} y={H - 12} textAnchor="middle" fontSize={12.5} fontWeight={600} style={{ fill: 'var(--text-secondary)' }}>Time, × the fastest (log) → slower</text>
            <text transform={`translate(16,${(T + H - B) / 2}) rotate(-90)`} textAnchor="middle" fontSize={12.5} fontWeight={600} style={{ fill: 'var(--text-secondary)' }}>Memory, × the leanest (log) → heavier</text>

            {front.length > 1 && <polyline points={front.map((p) => `${xs(p.x)},${ys(p.y)}`).join(' ')} fill="none" strokeWidth={1.5} strokeDasharray="5 4" style={{ stroke: 'var(--muted)' }} />}

            {trailPt && trailPt.per.map((p) => (
              <g key={p.op.key} onMouseMove={(e) => tip.show(e, (
                <><TipHead swatch={model.colors[trailPt.lib]}>{labelOf(model, trailPt.lib)}</TipHead><TipSub>{p.op.title}</TipSub>
                  <TipLine k="Mean time">{fmtMs(p.t)} · {fmtX(p.x)}</TipLine><TipLine k="Allocated">{fmtMB(p.m)} · {fmtX(p.y)}</TipLine></>))} onMouseLeave={tip.hide}>
                <line x1={xs(trailPt.x)} y1={ys(trailPt.y)} x2={xs(p.x)} y2={ys(p.y)} strokeOpacity={0.35} style={{ stroke: model.colors[trailPt.lib] }} />
                <circle cx={xs(p.x)} cy={ys(p.y)} r={4} fillOpacity={0.55} style={{ fill: model.colors[trailPt.lib] }} />
                <text x={xs(p.x) + 7} y={ys(p.y) + 4} fontSize={10.5} style={{ fill: 'var(--text-secondary)' }}>{p.op.title.split(' ').slice(0, 3).join(' ')}</text>
              </g>))}

            {pts.map((p) => {
              const side = sides.get(p.lib) ?? 'right', cx = xs(p.x), cy = ys(p.y), r = trail === p.lib ? 10 : 8
              return (
                <g key={p.lib} className="cursor-pointer" role="button" tabIndex={0} aria-pressed={trail === p.lib} aria-label={`${labelOf(model, p.lib)}: trace its operations`}
                  onClick={() => setTrail(trail === p.lib ? null : p.lib)}
                  onKeyDown={(e) => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); setTrail(trail === p.lib ? null : p.lib) } }}
                  onMouseMove={(e) => tip.show(e, pointTip(p))} onMouseLeave={tip.hide}>
                  <circle cx={cx} cy={cy} r={r} strokeWidth={2.5} style={{ fill: p.full ? model.colors[p.lib] : 'var(--surface)', stroke: model.colors[p.lib] }} />
                  <text x={side === 'above' || side === 'below' ? cx : side === 'left' ? cx - r - 5 : cx + r + 5} y={side === 'above' ? cy - r - 6 : side === 'below' ? cy + r + 14 : cy + 4} textAnchor={side === 'above' || side === 'below' ? 'middle' : side === 'left' ? 'end' : 'start'} fontSize={12.5} fontWeight={600} style={{ fill: 'var(--text-primary)' }}>{labelOf(model, p.lib)}</text>
                </g>
              )
            })}
          </svg>
        )}
      </div>
      <p className="mt-2.5 max-w-[90ch] text-[12.5px] text-muted">
        {front.length > 0 && <>Dashed line: libraries nobody beats on both axes ({front.map((p) => labelOf(model, p.lib)).join(', ')}). </>}
        {scope === 'all' && 'Hollow points did not complete every operation, so their mean covers fewer.'}
      </p>
    </section>
  )
}
