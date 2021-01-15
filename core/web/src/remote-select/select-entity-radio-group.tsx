import * as React from "react"
import { useGlowQuery } from "../query/use-data"
import { Radio } from "formik-antd"

interface Option {
  value: string | number
  label: React.ReactNode
}

export type SelectEntityPropsRadioGroup<T> = {
  url: string
  name: string
  map: (v: T) => Option
}

export function SelectEntityRadioGroup<T>({
  url,
  name,
  map,
  ...restProps
}: SelectEntityPropsRadioGroup<T>) {
  const [{ result, setSearch }, {}] = useGlowQuery<T>(url, {
    count: 0,
    value: [],
  })

  const options = result.value.map(map)

  return (
    <Radio.Group name={name}>
      {options.map((v) => (
        <Radio.Button name={name} value={v.value}>
          {v.label}
        </Radio.Button>
      ))}
    </Radio.Group>
  )
}
