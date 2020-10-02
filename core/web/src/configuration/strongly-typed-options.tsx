import * as React from "react"
import { Formik } from "formik"
import { message, PageHeader, Button, Card, Tooltip } from "antd"
import {
  Input,
  Switch,
  InputNumber,
  SubmitButton,
  Form,
  Table,
  AddRowButton,
  RemoveRowButton,
} from "formik-antd"
import { useActions, badRequestResponseToFormikErrors } from "./validation"
import { useData } from "../query/use-data"
import { ErrorBanner } from "../errors/error-banner"
import styled from "styled-components"

function toType(type: string, name: string, isArray: boolean) {
  switch (type) {
    case "string":
      return <Input fast={true} name={name} />
    case "number":
      return <InputNumber fast={true} name={name} />
    case "boolean":
      return <Switch fast={true} name={name} />
    case "object": {
      if (isArray) {
        return (
          <div>
            <AddRowButton
              name={name}
              style={{ marginBottom: 12 }}
              createNewRow={() => ""}
            >
              Add
            </AddRowButton>
            <Table
              name={name}
              columns={[
                {
                  render: (text, record, i) => (
                    <Row>
                      <Input
                        fast={true}
                        name={`${name}[${i}]`}
                        // style={{ flex: 1 }}
                      />
                      <RemoveRowButton name={`${name}`} index={i}>
                        remove
                      </RemoveRowButton>
                    </Row>
                  ),
                },
              ]}
              size="small"
              showHeader={false}
              pagination={false}
              bordered={false}
              style={{ width: 600 }}
            />
          </div>
        )
      }
      return <ErrorBanner error={`Type '${type}' not supported`}></ErrorBanner>
    }
    default:
      return <ErrorBanner error={`Type '${type}' not supported`}></ErrorBanner>
  }
}

const Row = styled.div`
  display: flex;

  > *:not(:first-child) {
    margin-left: 1rem;
  }
`

interface Props {
  title: string
  url: string
  configurationId: string
  allowEdit?: boolean
  name?: string
}

type WithChildren = Props & { type: "children"; children: React.ReactNode }
type WithEditors = Props & {
  type?: "editors"
  overrideEditors?: { [key: string]: React.ReactNode }
}
type P = WithChildren | WithEditors

export function StronglyTypedOptions({
  title,
  url,
  configurationId,
  allowEdit = true,
  name = "",
  ...rest
}: P) {
  const { submit } = useActions(url)
  const { data, error, refetch } = useData<any>(
    name ? url + "/" + name : url,
    {},
  )
  // Todo: use to auto generate ui
  const { data: typeDescription } = useData<any>(url + "/__description", {})
  const overrideEditors =
    rest.type === "editors" || rest.type === undefined
      ? rest.overrideEditors
      : undefined
  const children = rest.type === "children" ? rest.children : undefined
  return (
    <Card>
      <ErrorBanner error={error} />
      <Formik
        initialValues={data}
        enableReinitialize={true}
        onSubmit={async (values, actions) => {
          actions.setSubmitting(true)
          const r = await submit({ configurationId, value: values, name })
          actions.setSubmitting(false)
          if (r.ok) {
            refetch()
          } else {
            if (r.status === 400) {
              const errors = await r.json()
              actions.setErrors(
                (badRequestResponseToFormikErrors(errors) as any).value,
              )
            } else {
              message.error(r.statusText)
            }
          }
        }}
      >
        {(f) => (
          <Form labelAlign="left" labelCol={{ xs: 4 }}>
            <PageHeader
              title={title}
              extra={[
                allowEdit && (
                  <SubmitButton disabled={!f.dirty} key="submit">
                    Save
                  </SubmitButton>
                ),
                <Button
                  key="refresh"
                  onClick={() => {
                    refetch()
                  }}
                >
                  Refresh
                </Button>,
              ]}
            >
              <br />
              <br />
              <div>
                {children
                  ? children
                  : data &&
                    Object.keys(data).map((v) => (
                      <Form.Item
                        name={v}
                        htmlFor={name}
                        label={<b>{prettify(v)}</b>}
                        colon={false}
                        style={{ marginBottom: 5 }}
                      >
                        {overrideEditors && Boolean(overrideEditors[v])
                          ? overrideEditors[v]
                          : toType(typeof data[v], v, Array.isArray(data[v]))}
                      </Form.Item>
                    ))}
              </div>
            </PageHeader>
          </Form>
        )}
      </Formik>
    </Card>
  )
}

function prettify(val: string) {
  return capitalize(val.replace(/([a-z])([A-Z])/g, "$1 $2"))
}

function capitalize(s: string) {
  if (typeof s !== "string") return ""
  return s.charAt(0).toUpperCase() + s.slice(1)
}
