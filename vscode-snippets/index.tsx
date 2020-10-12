import * as React from "react"
import { Route, useNavigate, Routes } from "react-router-dom"
import { Button, PageHeader } from "antd"
import { useTranslation } from "react-i18next"
import {} from "@ant-design/icons"
import styled from "styled-components"
import { $1CreateView } from "./create"
import { $1ListView } from "./list"
import { $1DetailView } from "./detail"

const constants = {
  paths: {
    list: "/$1/",
    create: "/$1/create",
    id: "/$1/:id",
  },
}

const { paths } = constants

export function $1MasterDetailView() {
  const navigate = useNavigate()
  const { t } = useTranslation()
  return (
    <Container>
      <Routes>
        <Route path={paths.create} element={<$1CreateView />} />
        <Route
          path={paths.id}
          element={
            <MasterDetailContainer>
              <div>
                <Button
                  style={{ marginBottom: 10 }}
                  type="primary"
                  onClick={() => {
                    navigate(paths.create)
                  }}
                >
                  {t(\"new\")}
                </Button>
                <$1ListView />
              </div>

              <$1DetailView />
            </MasterDetailContainer>
          }
        />
        <Route
          path={paths.list}
          element={
            <>
              <Header
                title="$1"
                extra={[
                  <Button
                    style={{ marginBottom: 10 }}
                    type="primary"
                    // icon={<FileAddOutlined />}
                    onClick={() => {
                      navigate(paths.create)
                    }}
                  >
                    {t("new")}
                  </Button>,
                ]}
              />

              <$1ListView />
            </>
          }
        />
      </Routes>
    </Container>
  )
}

const Header = styled(PageHeader)``

const MasterDetailContainer = styled.div`
  display: grid;
  grid-template-columns: 1fr 1fr;
  grid-gap: 10;
  flex: 1;
`

const Container = styled.div``