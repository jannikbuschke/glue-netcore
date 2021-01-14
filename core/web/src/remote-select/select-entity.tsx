import * as React from "react"
import { Select } from "antd"
import { useField } from "formik"
import debounce from "lodash.debounce"
import { SelectProps } from "antd/lib/select"
import { LabeledValue } from "antd/lib/tree-select"
import { useGlowQuery } from "../query/use-data"

export type SelectEntityProps<T> = {
  url: string
  name: string
  map: (v: T) => LabeledValue
} & Omit<SelectProps<LabeledValue>, "fetcher">

export function SelectEntity<T>({
  url,
  name,
  map,
  ...restProps
}: SelectEntityProps<T>) {
  const [{ result, setSearch }, {}] = useGlowQuery<T>(url, {
    count: 0,
    value: [],
  })

  const options = result.value.map(map)

  const debouncedSearch = React.useCallback(
    debounce((v: any) => {
      ;(async () => {
        setSearch(v)
      })()
    }, 150),
    [],
  )

  React.useEffect(() => {
    debouncedSearch("")
    // TODO
    // on firstRender without full value
    // get single element
    // where is endpoint of single element?
  }, [])
  const [field, , form] = useField<any>(name)
  const value = field.value
  return (
    <Select<LabeledValue>
      dropdownMatchSelectWidth={false}
      value={field.value}
      onChange={(value) => {
        if (value === undefined) {
          form.setValue(null)
        }
      }}
      onBlur={() => {
        form.setTouched(true)
      }}
      style={{ width: "100%", ...restProps?.style }}
      showSearch={true}
      onDeselect={(v) => {
        if (restProps.mode === "multiple") {
          form.setValue((value as any[]).filter((item) => item != v))
        }
      }}
      onSelect={(v) => {
        if (v === undefined) {
          return
        }
        if (restProps.mode === "multiple") {
          form.setValue([...(value || []), v])
        } else {
          form.setValue(v)
        }
      }}
      onSearch={(search: string) => {
        debouncedSearch(search)
      }}
      filterOption={false}
      {...restProps}
      id={name}
    >
      {options.map((v) => (
        <Select.Option key={v.key || v.value} value={v.value}>
          {v.label}
        </Select.Option>
      ))}
    </Select>
  )
}
