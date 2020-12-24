import * as React from "react"
import { Card, Descriptions, Divider } from "antd"
import { useParams } from "react-router"
import { useData } from "../query/use-data"
import { constants } from "./constants"
import dayjs from "dayjs"
import styled from "styled-components"

export function AllConfigurationsDetailView() {
  const originalParams = useParams()
  const { id, name, version } = JSON.parse(decodeURI(originalParams["id"]))
  const { data, status, error } = useData<AllConfigurationsDto>(
    constants.api.single(id, version, name),
    {
      created: "",
      values: {},
      id: "",
      message: "",
      name: "",
      user: null,
      version: 0,
    },
  )

  return (
    <Container>
      <Card
        loading={(status as any) === "loading"}
        title={data.id}
        size="small"
        bordered={true}
        style={{ marginTop: 10 }}
      >
        <Descriptions bordered={true} size="small">
          <Descriptions.Item label="id" span={3}>
            {data.id}
          </Descriptions.Item>
          <Descriptions.Item label="name" span={3}>
            {data.name}
          </Descriptions.Item>
          <Descriptions.Item label="created" span={3}>
            {dayjs(data.created).format("LLL")}
          </Descriptions.Item>
          <Descriptions.Item label="version" span={3}>
            {data.version}
          </Descriptions.Item>
        </Descriptions>
        <Divider type="horizontal" />
        <Pre>{JSON.stringify(data ? data.values : undefined, null, 4)}</Pre>
      </Card>
    </Container>
  )
}

interface AllConfigurationsDto {
  id: string
  name: string
  version: number
  values: any
  created: string
  user: null
  message: string
}

const Pre = styled.pre`
  white-space: pre-wrap;
  word-wrap: break-word;
`

const Container = styled.div`
  padding: 50px;
`
