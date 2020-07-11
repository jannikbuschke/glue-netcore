import * as React from "react"
import { Formik } from "formik"
import { useSubmit, HtmlText } from "glow-react"
import { StageFiles, Files } from "glow-react/es/files/upload-files"
import { Form, Input, SubmitButton } from "formik-antd"
import { notification, Button, Card, Col, Table } from "antd"
import {
  Routes,
  Route,
  Link,
  useParams,
  Outlet,
  useNavigate,
} from "react-router-dom"
import { useData } from "glow-react/es/query/use-data"
import { HighlightableRow } from "glow-react/es/antd/highlightable-row"
import { ActionButton } from "glow-react/es/antd/action-button"
import { ActionBar } from "./layout"
import { IPartialConfiguration } from "./models"
import styled from "styled-components"
import { StronglyTypedOptions } from "glow-react/es/configuration/strongly-typed-options"

export function ConfigurationsExample() {
  return (
    <Routes>
      <Route path="configurations" element={<MasterDetail />}>
        <Route path=":configurationId" element={<Detail />} />
      </Route>
    </Routes>
  )
}

function Detail() {
  const { configurationId } = useParams()
  const url = `/api/configurations/${configurationId}`
  const { data } = useData<any>(url, {})
  //   const navigate = useNavigate()
  //   const [update] = useSubmit("/api/portfolios/update")
  return (
    <div style={{ maxWidth: 500 }}>
      <div>/detail</div>
      {/* <div>
        <pre>{JSON.stringify({ configurationId, data }, null, 4)}</pre>
      </div> */}
      <StronglyTypedOptions
        url={url}
        title="Sample"
        configurationId={configurationId}
      />
    </div>
  )
}

function MasterDetail() {
  return (
    <MasterDetailContainer>
      <List />
      <Outlet />
    </MasterDetailContainer>
  )
}

const MasterDetailContainer = styled.div`
  display: grid;
  grid-template-columns: 1fr 1fr;
  grid-gap: 10;
`

function List() {
  const url = "/api/__configuration/partial-configurations"
  const { data, refetch } = useData<IPartialConfiguration[]>(url, [])
  const navigate = useNavigate()
  return (
    <div>
      <div>/list</div>
      <div>{JSON.stringify(data, null, 4)}</div>
      <ActionBar>
        <Button onClick={() => refetch()}>Refresh</Button>
        <Button>
          <Link to="create">Create</Link>
        </Button>
      </ActionBar>
      <Table
        dataSource={data}
        loading={!Boolean(data)}
        showHeader={false}
        size="small"
        pagination={false}
        rowKey={(row) => row.route!}
        onRow={(row) => ({
          onClick: () => {
            navigate(`${row.id}`)
          },
        })}
        components={{
          body: {
            row: (props: any) => (
              <HighlightableRow path="/configurations/" {...props} />
            ),
          },
        }}
        columns={[
          {
            dataIndex: "title",
          },
        ]}
      />
    </div>
  )
}

function CreateOrUpdate() {
  return (
    <Card title={<HtmlText name="displayName" />} size="small">
      <Form labelCol={{ xs: 6 }} colon={false}>
        <Form.Item name="displayName" label="Displayname">
          <Input name="displayName" placeholder="displayName" />
        </Form.Item>
        <Form.Item name="files" label="Files">
          <Files
            name="files"
            fileUrl={(id) => `/api/portfolio/file-data/${id}`}
          />
          <StageFiles name="files" url="/api/portfolios/stage-files" />
        </Form.Item>
        <Col offset={6}>
          <SubmitButton>Submit</SubmitButton>
        </Col>
      </Form>
    </Card>
  )
}
