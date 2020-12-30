import React from "react"
import { QueryOptions, useQuery, UseQueryResult } from "react-query"
import { useFetchJson } from "../http/fetch"
import { OrderBy, Query, QueryResult } from "../ts-models"

export function useData<T>(
  url: string,
  placeholder: T,
  config?: QueryOptions<T>,
) {
  const fetchJson = useFetchJson<T>()
  const { data, ...rest } = useQuery<T, any>(url, () => fetchJson(url), {
    retry: 1,
    refetchOnWindowFocus: false,
    ...config,
  })
  return {
    data: Boolean(data) ? data! : placeholder,
    loading: rest.status === "loading",
    reload: rest.refetch,
    ...rest,
  }
}

export type UseGlowQueryResult<T> = [
  UseGlowQuery<T>,
  UseQueryResult<QueryResult<T>, any>,
]

export interface UseGlowQuery<T> {
  result: QueryResult<T>
  skip: number
  take: number
  search: string | null
  setSearch: React.Dispatch<React.SetStateAction<string | null>>
  setSkip: React.Dispatch<React.SetStateAction<number>>
  setTake: React.Dispatch<React.SetStateAction<number>>
  setOrderBy: React.Dispatch<React.SetStateAction<null | OrderBy>>
}

export function useGlowQuery<T>(
  url: string,
  placeholder: QueryResult<T>,
  config?: QueryOptions<QueryResult<T>>,
): UseGlowQueryResult<T> {
  const fetch = useFetchJson<QueryResult<T>>()
  const [take, setTake] = React.useState(10)
  const [skip, setSkip] = React.useState(0)
  const [orderBy, setOrderBy] = React.useState<null | OrderBy>(null)
  const [search, setSearch] = React.useState<null | string>(null)

  const useQueryResult = useQuery<QueryResult<T>, any>(
    url,
    async () => {
      const query: Query = {
        orderBy: (orderBy as any) as OrderBy,
        where: null as any,
        search,
        // orderBy: { direction: "Asc", property: "Id" },
        // where: {
        //   operation: "Contains",
        //   property: "",
        //   value: "",
        // },
        skip,
        take,
        count: true,
      }
      const result = await fetch(url, {
        method: "POST",
        headers: { "content-type": "application/json" },
        body: JSON.stringify(query),
      })
      return result
    },
    config,
  )
  React.useEffect(() => {
    useQueryResult.refetch()
  }, [take, skip, orderBy, search])

  const data = useQueryResult.data
  return [
    {
      result: Boolean(data) ? data! : placeholder,
      skip,
      take,
      setSkip,
      setTake,
      search,
      setSearch,
      setOrderBy,
    },
    useQueryResult,
  ]
}
