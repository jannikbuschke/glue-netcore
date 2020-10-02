/* eslint-disable prettier/prettier */
import { CreatePortfolio, UpdatePortfolio, DeletePortfolio, Unit } from "./models"
export module Portfolios {
  export async function PortfoliosCreate(request: CreatePortfolio) {
    const response = await fetch(`/api/portfolios/create?api-version=1.0`, {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify(request),
    })
    const data = await response.json()
    return data
  }
}
export module Portfolios {
  export async function PortfoliosUpdate(request: UpdatePortfolio) {
    const response = await fetch(`/api/portfolios/update?api-version=1.0`, {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify(request),
    })
    const data = await response.json()
    return data
  }
}
export module Portfolios {
  export async function PortfoliosDelete(request: DeletePortfolio) {
    const response = await fetch(`/api/portfolios/delete?api-version=1.0`, {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify(request),
    })
    const data = await response.json()
    return data
  }
}
export module Portfolios {
  export async function PortfoliosStage(request: Unit) {
    const response = await fetch(`/api/portfolios/stage-files?api-version=1.0`, {
      method: "POST",
      headers: { "content-type": "application/json" },
      body: JSON.stringify(request),
    })
    const data = await response.json()
    return data
  }
}
export module Portfolios {
  export async function PortfoliosExamples() {
    const response = await fetch(`/api/portfolios/examples?api-version=1.0`)
    const data = await response.json()
    return data
  }
}
export module Portfolios {
  export async function PortfoliosList() {
    const response = await fetch(`/api/portfolios/list?api-version=1.0`)
    const data = await response.json()
    return data
  }
}
export module Portfolios {
  export async function PortfoliosSingle(id: number) {
    const response = await fetch(`/api/portfolios/${id}?api-version=1.0`)
    const data = await response.json()
    return data
  }
}
export module Schema {
  export async function SchemaGet() {
    const response = await fetch(`/api/configuration-schemas?api-version=1.0`)
    const data = await response.json()
    return data
  }
}
