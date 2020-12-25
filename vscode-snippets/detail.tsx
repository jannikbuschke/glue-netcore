import * as React from "react"
import { useParams } from "react-router"
import {} from "@ant-design/icons"
import { Formik } from "formik"
import { Form, Input, SubmitButton } from "formik-antd"
import { useSubmit } from "glow-react/es/Forms/use-submit"
import { useData } from "glow-react/es/query/use-data"
import { Container, Header } from "../layout"
import { $1, default$1, Update$1 } from "../ts-models"

export function $1DetailView() {
  const { id } = useParams()
  const { data } = useData<$1>(`/api/$1/${id}`, default$1)
  const [update, validate] = useSubmit("/api/$1/update")
  return (
    <Container>
      <Header title={data.displayName} />
      <Formik<Update$1>
        initialValues={data}
        validate={validate}
        onSubmit={async (values) => {}}
      >
        <Form>
          <Form.Item name="displayName">
            <Input name="displayName" placeholder="Displayname" />
          </Form.Item>
          <SubmitButton>Save</SubmitButton>
        </Form>
      </Formik>
    </Container>
  )
}
