import { notification } from "antd"
import * as React from "react"
import { Route, Routes } from "react-router"
import { SelectAsync as RemoteSelect } from "glow-react"
import { Form, FormikDebug } from "formik-antd"
import { Formik } from "formik"
import { User } from "../ts-models"
import styled from "styled-components"
import { LabeledValue } from "antd/lib/select"

function Sample() {
  return (
    <Container>
      <Formik
        initialValues={{
          user: null,
          users: [],
        }}
        onSubmit={() => {}}
      >
        <Form layout="vertical">
          <Form.Item name="user" label="Single select">
            <RemoteSelect name="user" fetcher={getUsers} />
          </Form.Item>
          <Form.Item name="user" label="Multi select">
            <RemoteSelect name="users" mode="multiple" fetcher={getUsers} />
          </Form.Item>
          <FormikDebug />
        </Form>
      </Formik>
    </Container>
  )
}

export const constants = {
  path: "select-async",
}

export function SelectAsyncExample() {
  return (
    <Routes>
      <Route path={constants.path} element={<Sample />} />
    </Routes>
  )
}

const Container = styled.div`
  flex: 1;
`

async function getUsers(search: string) {
  try {
    const response = await fetch(`/api/user?search=${search}`)
    if (!response.ok) {
      notification.error({ message: response.status })
      return []
    }
    const data: User[] = await response.json()
    const labels = data.map(
      (v) =>
        ({
          key: v.id,
          value: v.id,
          label: <b>{v.displayName}</b>,
        } as LabeledValue),
    )
    return labels
  } catch (E) {
    notification.error({ message: E.toString() })
    throw E
  }
}
