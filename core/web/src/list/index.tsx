import { Input, Table } from "antd"
import { InputProps } from "antd/lib/input"
import { TableProps } from "antd/lib/table"
import * as React from "react"
import { useNavigate } from "react-router"
import { HighlightableRow } from "../antd/highlightable-row"
import { ErrorBanner } from "../errors/error-banner"
import { useListContext } from "./list-context"

interface ListProps<T> {
  path: string
  columns: {
    title: string
    key: string
    render: (item: T) => React.ReactNode
    sortable?: boolean
  }[]
}

export function ListSearch(props: Omit<InputProps, "value" | "onChange">) {
  const ctx = useListContext()
  const [{ search, setSearch }] = ctx.glowQuery

  return (
    <Input
      {...props}
      value={search || ""}
      onChange={(v) => setSearch(v.target.value)}
    />
  )
}

export function ListError() {
  const { glowQuery } = useListContext()
  const [, { error }] = glowQuery
  return error ? <ErrorBanner error={error} /> : null
}

export function List<RecordType extends { id: string } = any>({
  path,
  columns,
  ...props
}: ListProps<RecordType> &
  Omit<
    TableProps<RecordType>,
    | "columns"
    | "loading"
    | "rowKey"
    | "components"
    | "onRow"
    | "dataSource"
    | "onChange"
    | "pagination"
  >) {
  const navigate = useNavigate()

  const ctx = useListContext()
  const [
    { result: data, setOrderBy, skip, take, setSkip, setTake },
    { isLoading },
  ] = ctx.glowQuery
  // const [
  //   { result: data, setOrderBy, skip, take, setSkip, setTake },
  //   { isLoading },
  // ] = useGlowQuery<any>(baseUrl, {
  //   value: [],
  //   count: null,
  // })

  const pageSize = take
  const currentPage = skip / pageSize + 1

  return (
    <Table<RecordType>
      loading={isLoading}
      rowKey={(row) => row.id}
      components={{
        body: {
          row: (props: any) => <HighlightableRow path={path} {...props} />,
        },
      }}
      onRow={(record) => ({
        onClick: () => navigate(path + record.id),
      })}
      dataSource={data.value}
      onChange={(pagination, filters, sorter) => {
        if (Array.isArray(sorter)) {
          // not implemented
        } else {
          const { columnKey: property, order } = sorter
          if (!order) {
            setOrderBy(null)
          } else {
            setOrderBy({
              property: property as string,
              direction:
                order === "ascend"
                  ? "Asc"
                  : order === "descend"
                  ? "Desc"
                  : "Asc",
            })
          }
        }
      }}
      pagination={{
        current: currentPage,
        pageSize: pageSize,
        onChange: (page, size) => {
          if (size) {
            setTake(size)
          }
          setSkip((page - 1) * pageSize)
        },
        onShowSizeChange: (page, size) => {
          setSkip(size)
        },
        total: data.count || 10,
      }}
      columns={columns?.map((v) => ({
        title: v.title,
        key: v.key,
        sorter: v.sortable,
        render: (item, record) => v.render(record),
        // ...v,
        // filterDropdown: true,
        // filters: [
        //   {
        //     text: "Joe",
        //     value: "Joe",
        //   },
        // ],
        // filterDropdown: () => <div>hello world</div>,
        // filterDropdownVisible: true,
        // onFilter: (value, record) => false,
        // sorter: true,
      }))}
      {...props}
    />
  )
}

List.Error = ListError

List.Search = ListSearch
