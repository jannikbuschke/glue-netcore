import * as React from "react"
import { useGlowQuery, UseGlowQueryResult } from "../query/use-data"

interface IListContext<T> {
  glowQuery: UseGlowQueryResult<T>
}

const context = React.createContext<IListContext<any>>(null as any)

export function ListContext({
  url,
  children,
}: React.PropsWithChildren<{ url: string }>) {
  const glowQuery = useGlowQuery<any>(url, {
    value: [],
    count: null,
  })

  return <context.Provider value={{ glowQuery }}>{children}</context.Provider>
}

export function useListContext<T = any>() {
  const ctx = React.useContext(context)
  if (ctx === null) {
    throw new Error("cannot useListContext out of context")
  }
  return ctx as IListContext<T>
}
