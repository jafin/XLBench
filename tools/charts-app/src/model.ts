// The payload's shape as types, and the derived model every view reads.
// scripts/prepare-data.mjs checks the payload at build time; these types keep the code that reads it honest.

export interface Scenario {
  key: string
  label: string
  libraries: string[]
  snapshotOf?: (string | null)[]
  timeMs: (number | null)[]
  allocMb: (number | null)[]
  errorMs?: (number | null)[]
  stdDevMs?: (number | null)[]
}

export interface Payload {
  updated: string
  versions: Record<string, string>
  commercial?: string[]
  snapshots?: Record<string, string>
  scenarios: Scenario[]
}

export type Measure = 'time' | 'mem'

export interface Op { key: string; tag: string; title: string }
export interface Cell { lib: string; op: string; time: number | null; mem: number | null; err: number | null; sd: number | null }

export interface Model {
  updated: string
  versions: Record<string, string>
  libs: string[]
  colors: Record<string, string>
  ops: Op[]
  cells: Cell[]
  byOp: Record<string, Record<string, Cell>>
  snaps: Record<string, string>
  commercial: string[]
}

export const SNAP_MARK = '⧗'

export function buildModel(p: Payload): Model {
  const libs = Object.keys(p.versions)
  const colors = Object.fromEntries(libs.map((l, i) => [l, `var(--s${(i % 10) + 1})`]))
  const ops: Op[] = p.scenarios.map((s) => {
    const parts = s.label.split('·')
    return parts.length > 1
      ? { key: s.key, tag: parts[0]!.trim(), title: parts.slice(1).join('·').trim() }
      : { key: s.key, tag: '', title: s.label }
  })
  const cells: Cell[] = p.scenarios.flatMap((s) =>
    s.libraries.map((lib, i) => ({
      lib, op: s.key, time: s.timeMs[i] ?? null, mem: s.allocMb[i] ?? null, err: s.errorMs?.[i] ?? null, sd: s.stdDevMs?.[i] ?? null,
    })))
  const byOp: Model['byOp'] = {}
  for (const c of cells) (byOp[c.op] ??= {})[c.lib] = c
  return { updated: p.updated, versions: p.versions, libs, colors, ops, cells, byOp,
    snaps: p.snapshots ?? {}, commercial: p.commercial ?? [] }
}

export const isNum = (v: number | null | undefined): v is number => typeof v === 'number' && Number.isFinite(v) && v > 0
export const labelOf = (m: Model, lib: string) => (lib in m.snaps ? `${lib} ${SNAP_MARK}` : lib)

export const fmtMs = (v: number) =>
  v >= 1000 ? `${(v / 1000).toLocaleString(undefined, { maximumFractionDigits: 2 })} s`
            : `${v.toLocaleString(undefined, { maximumFractionDigits: v < 10 ? 1 : 0 })} ms`
export const fmtMB = (v: number) => `${v.toLocaleString(undefined, { maximumFractionDigits: v < 100 ? 1 : 0 })} MB`
export const fmtX = (r: number) =>
  r < 1.005 ? '1×' : r < 10 ? `${r.toFixed(r < 1.95 ? 2 : 1)}×` : `${Math.round(r).toLocaleString()}×`
export const fmtVal = (m: Measure, v: number | null) => (v == null ? '—' : m === 'time' ? fmtMs(v) : fmtMB(v))
