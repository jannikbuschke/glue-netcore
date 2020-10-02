import React from "react"
import { notification } from "antd"
import * as signalR from "@microsoft/signalr"
import { useMitt } from "../mitt"

export interface EntityChanged {
  key: { [key: string]: any }
  state: number
  entityName: string | null
}

export function useInitializeDbChanges() {
  const { emitter } = useMitt()
  React.useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl("/notifications/db-changes")
      .build()

    connection.on("DbChanged", (entityChanged: EntityChanged[]) => {
      emitter.emit("DbChanged", entityChanged)
    })

    connection
      .start()
      .catch((err) => notification.error({ message: err.toString() }))
  }, [])
}
