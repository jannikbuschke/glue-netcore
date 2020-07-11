import React from "react"
import "./App.css"
import { Tabs, Space } from "antd"
import { ApplicationLayout } from "glow-react/es/Layout/Layout"
import "antd/dist/antd.css"
import { FilesExample } from "./files-example"
import { BrowserRouter as Router, Link } from "react-router-dom"
import { ConfigurationsExample } from "./configuration-example"

function App() {
  return (
    <Router>
      <ApplicationLayout Header={null}>
        <Space>
          <Link to="/portfolios/">Portfolios</Link>
          <Link to="/configurations/">Configurations</Link>
        </Space>
        <FilesExample />
        <ConfigurationsExample />
        {/* <Tabs style={{ margin: 100 }}>
          <Tabs.TabPane tab="Portfolios">
          </Tabs.TabPane>
          <Tabs.TabPane tab="Configurations">
          </Tabs.TabPane>
        </Tabs> */}
      </ApplicationLayout>
    </Router>
  )
}

export default App
