import { ClientFunction } from "testcafe"

interface Query {
  take: number
  skip: number
  search: string
}

interface QueryResult<T> {
  value: T[]
}

function _query<T>(url: string, query: Query) {
  return new Promise<QueryResult<T>>((resolve) => {
    fetch(url, {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify(query),
    })
      .then((v) => {
        if (!v.ok) {
          throw new Error(`query '${url}' failed: ${v.statusText || v.status}`)
        }
        return v.json()
      })
      .then((v) => {
        resolve(v)
      })
  })
}

export function query<T>() {
  return ClientFunction<Promise<QueryResult<T>>, [string, Query]>(
    (url, query) => {
      return _query<T>(url, query)
    },
    { dependencies: { _query } },
  )
}
