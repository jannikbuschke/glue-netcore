import { Button, PageHeader, Tabs, Tag } from "antd"
import * as React from "react"
import { Route, Routes } from "react-router"
import { RoutedTabs, List } from "glow-react"
import { ListViewItem } from "../ts-models"
import { ListContext } from "glow-react/es/list/list-context"

export function ListViewExample() {
  return (
    <Routes>
      <Route path="list-view/*" element={<Sample />} />
    </Routes>
  )
}

function Sample() {
  return (
    <div style={{ flex: 1 }}>
      <PageHeader
        title="Listview"
        extra={[<Button>Save</Button>, <Button>Delete</Button>]}
        subTitle="subtitle"
        tags={[<Tag>foo</Tag>]}
        footer={
          <RoutedTabs baseUrl="/detail-view" parameter="sub" size="large">
            <Tabs.TabPane key="key1" tab="Tab1">
              <br />
              <ListContext url="/api/list-view/query">
                <List.Error />
                <List.Search placeholder="search" />
                <List<ListViewItem>
                  path="/list-view/"
                  style={{ marginTop: 5 }}
                  size="small"
                  columns={[
                    {
                      title: "Name",
                      key: "DisplayName",
                      sortable: true,
                      render: (record) => <span>{record.displayName}</span>,
                    },
                    {
                      title: "Birthday",
                      key: "Birthday",
                      sortable: true,
                      render: (record) => <span>{record.birthday}</span>,
                    },
                    {
                      title: "City",
                      key: "City",
                      render: (record) => <span>{record.city}</span>,
                    },
                  ]}
                />
              </ListContext>
            </Tabs.TabPane>
          </RoutedTabs>
        }
      ></PageHeader>
    </div>
  )
}
