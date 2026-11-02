// One tooltip for the whole app, shared by every chart. Content is JSX, so it is escaped by the renderer.
import { createContext, useCallback, useContext, useLayoutEffect, useMemo, useRef, useState } from 'react'
import type { MouseEvent, ReactNode } from 'react'

interface TipState { x: number; y: number; content: ReactNode }
interface TipApi { show: (e: MouseEvent, content: ReactNode) => void; hide: () => void }

const TipCtx = createContext<TipApi | null>(null)

export function TooltipProvider({ children }: { children: ReactNode }) {
  const [tip, setTip] = useState<TipState | null>(null)
  const [pos, setPos] = useState({ left: 0, top: 0 })
  const ref = useRef<HTMLDivElement>(null)

  const show = useCallback((e: MouseEvent, content: ReactNode) => setTip({ x: e.clientX, y: e.clientY, content }), [])
  const hide = useCallback(() => setTip(null), [])
  const api = useMemo<TipApi>(() => ({ show, hide }), [show, hide])

  // Follow the pointer, flipping to the other side when the tooltip would leave the viewport.
  useLayoutEffect(() => {
    const el = ref.current
    if (!tip || !el) return
    const pad = 14
    let left = tip.x + pad, top = tip.y + pad
    if (left + el.offsetWidth > window.innerWidth - 8) left = tip.x - el.offsetWidth - pad
    if (top + el.offsetHeight > window.innerHeight - 8) top = tip.y - el.offsetHeight - pad
    setPos({ left, top })
  }, [tip])

  return (
    <TipCtx.Provider value={api}>
      {children}
      <div ref={ref} role="tooltip" style={{ left: pos.left, top: pos.top, opacity: tip ? 1 : 0 }}
        className="pointer-events-none fixed z-50 min-w-[200px] rounded-[10px] border border-line bg-surface px-3 py-2.5 text-[12.5px] shadow-[0_8px_28px_rgba(0,0,0,0.18)] transition-opacity duration-100">
        {tip?.content}
      </div>
    </TipCtx.Provider>
  )
}

export function useTip(): TipApi {
  const c = useContext(TipCtx)
  if (!c) throw new Error('useTip outside <TooltipProvider>')
  return c
}

export function TipHead({ swatch, children }: { swatch?: string; children: ReactNode }) {
  return (
    <div className="mb-0.5 flex items-center gap-[7px] font-bold">
      {swatch && <i className="inline-block size-2.5 rounded-[3px] shadow-[0_0_0_1px_var(--line)_inset]" style={{ background: swatch }} />}
      {children}
    </div>
  )
}

export function TipSub({ children }: { children: ReactNode }) {
  return <div className="mb-2 text-[11px] uppercase tracking-wider text-muted">{children}</div>
}

export function TipLine({ k, children }: { k: ReactNode; children: ReactNode }) {
  return (
    <div className="flex justify-between gap-[22px] py-0.5 text-ink2">
      <span>{k}</span><b className="font-semibold tabular-nums text-ink">{children}</b>
    </div>
  )
}
