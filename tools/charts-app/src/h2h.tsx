// The head-to-head section: two libraries, operation by operation, as diverging bars around 1x.
// It reads every library, independent of the library toggles, because the pair is chosen explicitly here.
import { useApp } from './ctx'
import { fmtMB, fmtMs, fmtX, isNum, labelOf, type Cell, type Measure, type Op } from './model'
import { Swatch } from './views'
import { TipHead, TipLine, TipSub, useTip } from './tooltip'

interface Ratio { r: number; a: number; b: number }  // r > 1 means A is better (B's value over A's)
const FIELD = { time: 'time', mem: 'mem' } as const satisfies Record<Measure, keyof Cell>
const WORD: Record<Measure, string> = { time: 'faster', mem: 'leaner' }
const NAME: Record<Measure, string> = { time: 'Speed', mem: 'Memory' }
const fmt = (m: Measure, v: number) => (m === 'time' ? fmtMs(v) : fmtMB(v))

export function HeadToHead() {
  const { model, search, setSearch } = useApp()
  const tip = useTip()
  const { libs } = model
  const a = libs.includes(search.a) ? search.a : (libs.includes('XLibur') ? 'XLibur' : libs[0]!)
  const b = libs.includes(search.b) && search.b !== a ? search.b : (libs.includes('ClosedXML') && a !== 'ClosedXML' ? 'ClosedXML' : libs.find((l) => l !== a)!)

  const ratio = (op: Op, m: Measure): Ratio | null => {
    const ca = model.byOp[op.key]?.[a], cb = model.byOp[op.key]?.[b]
    const va = ca?.[FIELD[m]], vb = cb?.[FIELD[m]]
    return isNum(va) && isNum(vb) ? { r: vb / va, a: va, b: vb } : null
  }
  const rows = model.ops.map((op) => ({ op, time: ratio(op, 'time'), mem: ratio(op, 'mem') }))
  const maxLog = (m: Measure) => Math.max(0.1, ...rows.map((r) => (r[m] ? Math.abs(Math.log(r[m]!.r)) : 0)))
  const snaps = [a, b].filter((l) => l in model.snaps)

  const Summary = ({ m }: { m: Measure }) => {
    const rs = rows.map((r) => r[m]).filter((x): x is Ratio => !!x)
    if (!rs.length) return <Tile k={NAME[m]} big="—" small="No operation that both libraries completed." />
    const g = Math.exp(rs.reduce((s, x) => s + Math.log(x.r), 0) / rs.length)
    const aWins = rs.filter((x) => x.r > 1.005).length, bWins = rs.filter((x) => x.r < 0.995).length
    const who = g >= 1 ? a : b, mag = g >= 1 ? g : 1 / g
    return <Tile k={NAME[m]}
      big={mag < 1.005 ? 'Level overall' : <><Swatch color={model.colors[who]!} />{labelOf(model, who)} {fmtX(mag)} {WORD[m]}</>}
      small={`Geometric mean over ${rs.length} operation${rs.length > 1 ? 's' : ''}. ${labelOf(model, a)} ${WORD[m]} in ${aWins}, ${labelOf(model, b)} in ${bWins}${rs.length - aWins - bWins ? `, level in ${rs.length - aWins - bWins}` : ''}.`} />
  }

  const Bar = ({ x, m, op }: { x: Ratio | null; m: Measure; op: Op }) => {
    if (!x) return <div className="hatch relative h-6 rounded"><span className="absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 bg-surface px-1.5 text-[10.5px] font-bold tracking-wider text-muted">NO RESULT</span></div>
    const aBetter = x.r >= 1, mag = aBetter ? x.r : 1 / x.r, level = mag < 1.005
    const frac = Math.min(Math.log(mag) / maxLog(m), 1) * 36
    return (
      <div className="relative h-6 before:absolute before:inset-y-[-3px] before:left-1/2 before:w-[1.5px] before:bg-baseline"
        onMouseMove={(e) => tip.show(e, (
          <>
            <TipHead>{op.title}</TipHead><TipSub>{op.tag ? `${op.tag} · ` : ''}{NAME[m]}</TipSub>
            <TipLine k={<><Swatch color={model.colors[a]!} />{labelOf(model, a)}</>}>{fmt(m, x.a)}</TipLine>
            <TipLine k={<><Swatch color={model.colors[b]!} />{labelOf(model, b)}</>}>{fmt(m, x.b)}</TipLine>
            <TipLine k="Ratio">{level ? 'level' : `${fmtX(mag)} for ${labelOf(model, aBetter ? a : b)}`}</TipLine>
          </>))}
        onMouseLeave={tip.hide}>
        {!level && <span className="absolute inset-y-[3px] min-w-[2px] rounded" style={{ left: aBetter ? `${50 - frac}%` : '50%', width: `${frac}%`, background: model.colors[aBetter ? a : b] }} />}
        <span className="absolute top-1/2 text-xs font-semibold tabular-nums"
          style={level ? { left: '50%', marginLeft: 6, transform: 'translateY(-50%)' }
            : aBetter ? { left: `${50 - frac}%`, marginLeft: -6, transform: 'translate(-100%,-50%)' } : { left: `${50 + frac}%`, marginLeft: 6, transform: 'translateY(-50%)' }}>
          {level ? 'level' : fmtX(mag)}
        </span>
      </div>
    )
  }

  const Head = ({ m }: { m: Measure }) => (
    <div className="flex justify-between gap-2 border-b border-grid pb-1 text-[11.5px] font-bold uppercase tracking-wide text-muted">
      <span>◀ <Swatch color={model.colors[a]!} />{labelOf(model, a)} {WORD[m]}</span><span>{labelOf(model, b)} <Swatch color={model.colors[b]!} /> {WORD[m]} ▶</span>
    </div>
  )

  return (
    <section className="mt-7 rounded-2xl border border-line bg-surface p-4">
      <h2 className="text-[15px] font-semibold">Head-to-head</h2>
      <p className="mb-3 mt-1 max-w-[80ch] text-[13px] text-ink2">Pick two libraries and see how they compare on every operation. Each bar grows toward the library that is faster or leaner. This view always uses every library, whatever is toggled above.</p>
      <div className="mb-3 flex flex-wrap items-center gap-2">
        <LibSelect value={a} onChange={(v) => setSearch({ a: v, b: v === b ? libs.find((l) => l !== v)! : b })} />
        <span className="text-[12.5px] font-bold text-muted">vs</span>
        <LibSelect value={b} onChange={(v) => setSearch({ b: v, a: v === a ? libs.find((l) => l !== v)! : a })} />
        <button type="button" className="rounded-lg border border-line bg-surface px-3 py-1.5 text-[13px] font-medium text-ink2 hover:text-ink" onClick={() => setSearch({ a: b, b: a })}>Swap</button>
      </div>
      <div className="mb-4 grid gap-3 md:grid-cols-2"><Summary m="time" /><Summary m="mem" /></div>
      <div className="grid grid-cols-[minmax(150px,1.1fr)_2fr_2fr] items-center gap-x-4 gap-y-1.5 max-md:grid-cols-2">
        <div className="max-md:hidden border-b border-grid pb-1 text-[11.5px] font-bold uppercase tracking-wide text-muted">Operation</div>
        <Head m="time" /><Head m="mem" />
        {rows.map((r) => (
          <div key={r.op.key} className="contents">
            <div className="text-[13px] leading-tight max-md:col-span-2 max-md:mt-2"><i className="block text-[10.5px] font-bold not-italic uppercase tracking-wider text-muted">{r.op.tag}</i>{r.op.title}</div>
            <Bar x={r.time} m="time" op={r.op} /><Bar x={r.mem} m="mem" op={r.op} />
          </div>
        ))}
      </div>
      <p className="mt-3 border-t border-grid pt-3 text-[11.5px] text-muted">
        A ratio is the larger result divided by the smaller, so 3× means three times as slow (or as heavy).
        {snaps.length > 0 && <> <b>⧗</b> {snaps.join(' and ')} {snaps.length > 1 ? 'were' : 'was'} carried over from an earlier run, so treat time comparisons with care.</>}
      </p>
    </section>
  )

  function LibSelect({ value, onChange }: { value: string; onChange: (v: string) => void }) {
    return (
      <select value={value} onChange={(e) => onChange(e.target.value)} className="rounded-lg border border-line bg-surface px-2.5 py-1.5 text-[13px] font-medium text-ink">
        {libs.map((l) => <option key={l} value={l}>{labelOf(model, l)}</option>)}
      </select>
    )
  }
}

function Tile({ k, big, small }: { k: string; big: React.ReactNode; small: string }) {
  return (
    <div className="rounded-xl border border-line bg-page px-3.5 py-2.5">
      <div className="text-[11.5px] font-bold uppercase tracking-wider text-muted">{k}</div>
      <div className="my-0.5 text-[19px] font-semibold tracking-tight">{big}</div>
      <div className="text-[12.5px] text-ink2">{small}</div>
    </div>
  )
}
