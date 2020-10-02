import * as React from "react"
import { Button } from "antd"
import { Routes, Route } from "react-router-dom"
import { useData } from "glow-react/es/query/use-data"
import { EntityChanged, Portfolio, UpdatePortfolio } from "./models"
import { useMitt } from "glow-react/es/mitt"
import { submitJson } from "glow-react/es/Forms/use-submit"
import { Handler } from "mitt"
import { Entities } from "./my-models"

export function DbNotificationsExample() {
  return (
    <Routes>
      <Route path="db-notifications" element={<MasterDetail />}></Route>
    </Routes>
  )
}

function MasterDetail() {
  const { data, error, loading, refetch } = useData<Portfolio>(
    "/api/portfolio/1",
    {
      displayName: "",
      files: [],
      id: 0,
    },
  )
  const fo: Entities.Bar = "Glow.Sample.Test+Bar"
  useEntityChanges("Glow.Sample.Files.Portfolio", 1, refetch)
  return (
    <div>
      <Button
        onClick={async () => {
          await submitJson("/api/portfolios/update", {
            id: 1,
            displayName: "" + Math.random(),
          } as UpdatePortfolio)
        }}
      >
        update
      </Button>
      <div>
        {data.id} {data.displayName}
      </div>
    </div>
  )
}

function useEntityChanges(
  entityName: string,
  id: string | number,
  callback: (v?: any) => any | Promise<any>,
) {
  const { emitter } = useMitt()
  const onDbChanged: Handler<any> = React.useCallback(
    (v: EntityChanged[]) => {
      v.forEach((element) => {
        if (element.entityName === entityName && element.key["Id"] === id) {
          callback(element)
        }
      })
    },
    [entityName, id],
  )
  React.useEffect(() => {
    emitter.on("DbChanged", onDbChanged)
    return () => emitter.off("DbChanged", onDbChanged)
  }, [])
  return onDbChanged
}
