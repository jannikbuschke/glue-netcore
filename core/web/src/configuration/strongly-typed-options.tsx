import * as React from "react"
import useSWR from "swr"
import { Formik } from "formik"
import { message, Alert, PageHeader, Divider } from "antd"
import { Input, Switch, InputNumber, SubmitButton, Form } from "formik-antd"
import { useActions, badRequestResponseToFormikErrors } from "./validation"
import { useData } from "../query/use-data"

function toType(type: string, name: string) {
  switch (type) {
    case "string":
      return <Input fast={true} name={name} />
    case "number":
      return <InputNumber fast={true} name={name} />
    case "boolean":
      return <Switch fast={true} name={name} />
    default:
      return <Input fast={true} name={name} />
  }
}

interface Props {
  title: string
  url: string
  configurationId: string
}

export function StronglyTypedOptions({ title, url, configurationId }: Props) {
  const { submit } = useActions(url)
  // const { data, error, revalidate } = useSWR(url)
  const { data, error } = useData<any>(url, {})
  console.log({ data, error, url })
  return (
    <div style={{ maxWidth: 1200, background: "#fff" }}>
      {error && <Alert type="error" message={(error as any).toString()} />}
      <Formik
        initialValues={data}
        enableReinitialize={true}
        onSubmit={async (values, actions) => {
          console.log("submit")
          actions.setSubmitting(true)
          const r = await submit({ configurationId, value: values })
          actions.setSubmitting(false)
          if (r.ok) {
            message.success("success")
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
        <Form>
          <PageHeader
            title={title}
            extra={[
              <SubmitButton size="small">save</SubmitButton>,
              // TODO somehow formik does not show the new values on revalidating
              // <Button
              //   size="small"
              //   key={2}
              //   icon="reload"
              //   onClick={() => revalidate()}>
              //   refresh
              // </Button>,
            ]}
          >
            <Divider />
            <div
              style={{
                display: "grid",
                gridTemplateColumns: "160px auto",
              }}
            >
              {data &&
                Object.keys(data).map((v) => (
                  <>
                    <label
                      style={{
                        marginTop: 10,
                        marginRight: 10,
                        textAlign: "right",
                      }}
                    >
                      {v}
                    </label>
                    <Form.Item name={v}>{toType(typeof data[v], v)}</Form.Item>
                  </>
                ))}
            </div>
          </PageHeader>
        </Form>
      </Formik>
    </div>
  )
}
