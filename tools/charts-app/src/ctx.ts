import { createContext, useContext } from 'react'
import type { Measure, Model } from './model'

// URL-backed view state (TanStack Router search params): shareable, reload-stable, back-button friendly.
export interface Search { measure: Measure; hide: string; a: string; b: string }

export interface Ctx {
  model: Model
  search: Search
  active: string[]
  setSearch: (patch: Partial<Search>) => void
}

export const AppCtx = createContext<Ctx | null>(null)
export function useApp(): Ctx {
  const c = useContext(AppCtx)
  if (!c) throw new Error('useApp outside <AppCtx.Provider>')
  return c
}
