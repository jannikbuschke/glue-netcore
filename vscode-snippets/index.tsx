import * as React from "react"
import { Route,  Routes } from "react-router-dom"
import styled from "styled-components"
import { $1CreateView } from "./create"
import { $1ListView } from "./list"
import { $1DetailView } from "./detail"
import { $1Constants } from "./constants"

const { paths } = $1Constants

export function $1Routes() {
  return (
      <Routes>
        <Route path={paths.create} element={<$1CreateView />} />
        <Route
          path={paths.id}
          element={
            <MasterDetailContainer>
              <$1ListView />
              <$1DetailView />
            </MasterDetailContainer>
          }
        />
        <Route
          path={paths.list}
          element={<$1ListView />}
        />
      </Routes>
  )
}

const MasterDetailContainer = styled.div`
  display: grid;
  grid-template-columns: 1fr 1fr;
  grid-gap: 10;
  flex: 1;
`
