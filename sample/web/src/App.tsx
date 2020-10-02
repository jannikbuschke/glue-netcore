import React from "react"
import { Menu, notification, Space } from "antd"
import "antd/dist/antd.css"
import { FilesExample } from "./files-example"
import { BrowserRouter as Router, Link } from "react-router-dom"
import { ConfigurationsExample } from "./configuration-example"
import styled from "styled-components"
import { DbNotificationsExample } from "./db-notifications"
import { useInitializeDbChanges } from "glow-react/es/db-notifications"

function App() {
  useInitializeDbChanges()

  return (
    <Router>
      <Container>
        <div>
          <Menu mode="horizontal">
            <Menu.Item>
              <Link to="/files/">Files</Link>
            </Menu.Item>
            <Menu.Item>
              <Link to="/configurations/">Configurations</Link>
            </Menu.Item>
            <Menu.Item>
              <Link to="/db-notifications/">Db Notifications</Link>
            </Menu.Item>
          </Menu>
        </div>
        <Content>
          <FilesExample />
          <ConfigurationsExample />
          <DbNotificationsExample />
        </Content>
        {/* <Tabs style={{ margin: 100 }}>
          <Tabs.TabPane tab="Portfolios">
          </Tabs.TabPane>
          <Tabs.TabPane tab="Configurations">
          </Tabs.TabPane>
        </Tabs> */}
      </Container>
    </Router>
  )
}

const Container = styled.div`
  display: flex;
  flex-direction: column;
  height: 100vh;
`

const Content = styled.div`
  display: flex;
  flex: 1;
  padding: 40px;
`

export default App
