import { Button } from "antd"
import * as React from "react"
import { useData } from "../query/use-data"

export function GraphProfileView() {
  const { data, refetch } = useData<{ displayName: string; id: string }>(
    "/graph/profile",
    { displayName: "", id: "" },
  )

  return (
    <div>
      <Button onClick={() => refetch()}>reload</Button>
      <pre>{JSON.stringify(data, null, 4)}</pre>
    </div>
  )
}
