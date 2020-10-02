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
import { EntityChanged } from "./models"
import { useMitt } from "glow-react/es/mitt"

export function FilesExample() {
  return (
    <Routes>
      <Route path="files" element={<MasterDetail />}>
        <Route path="create" element={<Create />} />
        <Route path=":portfolioId" element={<Detail />} />
      </Route>
    </Routes>
  )
}

interface File {
  id: string
  name: string
}
interface Portfolio {
  id: string
  displayName: string
  files: File[]
}

function Detail() {
  const { portfolioId } = useParams()
  const { data } = useData<Portfolio>(
    `/api/portfolios/${portfolioId}?$expand=files`,
    {
      id: "",
      displayName: "",
      files: [],
    },
  )
  const navigate = useNavigate()
  const [update] = useSubmit("/api/portfolios/update")
  return (
    <div style={{ maxWidth: 500 }}>
      <div>/detail</div>
      <ActionBar>
        <ActionButton
          path="/api/portfolios/delete"
          payload={{ id: data.id }}
          danger={true}
          onSuccess={() => navigate("..")}
        >
          Delete
        </ActionButton>
      </ActionBar>
      <Formik
        initialValues={{
          id: portfolioId,
          displayName: data.displayName,
          files: data.files,
        }}
        enableReinitialize={true}
        onSubmit={async (values) => {
          const result = await update(values)
          if (result) {
            notification.success({ message: "success" })
          }
        }}
      >
        <CreateOrUpdate />
      </Formik>
    </div>
  )
}

function MasterDetail() {
  return (
    <div
      style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gridGap: 10 }}
    >
      <List />
      <Outlet />
    </div>
  )
}

function List() {
  const url = "/api/portfolios/list?$expand=files"
  const { data, refetch } = useData<Portfolio[]>(url, [])
  const { emitter } = useMitt()

  React.useEffect(() => {
    emitter.on("DbChanged", (v) => {
      const changes: EntityChanged[] = v
      console.log("changes", changes)
      refetch()
    })
  }, [])
  const navigate = useNavigate()
  return (
    <div>
      <div>/list</div>
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
        rowKey={(row) => row.id}
        onRow={(row) => ({
          onClick: () => {
            navigate(`${row.id}`)
          },
        })}
        components={{
          body: {
            row: (props: any) => (
              <HighlightableRow path="/portfolios/" {...props} />
            ),
          },
        }}
        columns={[
          {
            dataIndex: "displayName",
          },
        ]}
      />
    </div>
  )
}

function Create() {
  const [create] = useSubmit("/api/portfolios/create")
  return (
    <div style={{ maxWidth: 500 }}>
      <div>/create</div>
      <Formik
        initialValues={{ id: null, displayName: "", files: [] }}
        onSubmit={async (values) => {
          const result = await create(values)
          if (result) {
            notification.success({ message: "success" })
          }
        }}
      >
        <CreateOrUpdate />
      </Formik>
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
