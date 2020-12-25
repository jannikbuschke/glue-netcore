import * as React from "react"
import { useNavigate, useParams, useLocation } from "react-router-dom"
import { PageHeader } from "antd"
import {} from "@ant-design/icons"
import { MasterDetailView } from "glow-react/es/layouts/master-detail-view"
import { List } from "glow-react"
import { createBrowserHistory } from "history"
import { ListContext } from "glow-react/es/list/list-context"

export function MasterDetailViewExample() {
  return (
    <MasterDetailView
      path="/master-detail/"
      create={<div>create</div>}
      master={<Master />}
      detail={<Detail />}
    />
  )
}

const history = createBrowserHistory()

function Master() {
  return (
    <div style={{ flex: 1 }}>
      <PageHeader title="Master" onBack={() => history.back()} />
      <ListContext url="/api/list-view/data">
        <List<any>
          path="/master-detail/"
          style={{ marginTop: 5 }}
          size="small"
          columns={[
            {
              title: "Name",
              key: "Name",
              render: (row) => <span>{row.displayName}</span>,
            },
          ]}
        />
      </ListContext>
    </div>
  )
}

function Detail() {
  const { id } = useParams()
  return (
    <div>
      <h1>{id}</h1>
    </div>
  )
}
