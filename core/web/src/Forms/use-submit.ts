import * as React from "react"
import { set } from "lodash"

export type Result<T> = Success<T> | Error

export interface Success<T> {
  ok: true
  result: "success"
  payload: T
}

export interface Error {
  ok: false
  result: "error"
  error: ProblemDetails
}

export interface ValidationResult {
  isValid: boolean
  errors: SerializableError
}

export interface SerializableError {
  [key: string]: string[]
}

export interface ModelStateDictionary {
  root: any
  maxAllowedErrors: number
  hasReachedMaxErrors: boolean
  errorCount: number
  count: number
  keys: string | null[]
  values: any[]
  isValid: boolean
  validationState: number
  item: any
}

interface ProblemDetails {
  detail: string
  status: number
  title: string
  type: string
  extensions: any
}

export function useSubmit<T = any>(
  url: string,
): [
  (values: any) => Promise<Result<T>>,
  (values: any) => Promise<SerializableError | undefined>,
  string,
] {
  const [error, setError] = React.useState("")
  return [
    async (values: any): Promise<Result<T>> => {
      const response = await send(url, values, "execute")
      if (!response.ok) {
        if (response.headers.has("content-type")) {
          const contentType = response.headers.get("content-type")
          if (contentType == "application/problem+json") {
            const description = (await response.json()) as ProblemDetails
            setError(
              `${description.title}: ${description.detail} (${description.status})`,
            )
            return { error: description, ok: false, result: "error" } as Error
          }
        }
        console.error(response)
        return {
          error: {
            status: response.status,
            title: response.statusText,
            detail: "",
            type: "",
          } as ProblemDetails,
          ok: false,
          result: "error",
        } as Error
      } else {
        const data = (await response.json()) as T
        return { payload: data, ok: true, result: "success" } as Success<T>
      }
    },
    async (values: any): Promise<any | undefined> => {
      const response = await send(url, values, "validate")
      if (response.ok) {
        const data = (await response.json()) as ValidationResult
        const errors = toFormikErrors(data.errors)
        return errors
      } else {
        console.error(response)
        setError("error while validating: " + response.statusText)
        return
      }
    },
    error,
  ]
}

function camelize(str: string) {
  return str.split(".").map(_camelize).join(".")
}

function _camelize(str: string) {
  return str.replace(/(?:^\w|[A-Z]|\b\w|\s+)/g, function (match, index) {
    if (+match === 0) return "" // or if (/\s+/.test(match)) for white spaces
    return index == 0 ? match.toLowerCase() : match.toUpperCase()
  })
}

const toFormikErrors = (error: SerializableError) => {
  const errors = {}
  Object.keys(error).forEach((key) => {
    const path = camelize(key)
    set(errors, path, error[key])
  })
  return errors
}

function send(url: string, values: any, intent: "execute" | "validate") {
  return fetch(url, {
    method: "POST",
    body: JSON.stringify(values),
    headers: {
      "x-submit-intent": intent,
      "content-type": "application/json",
    },
  })
}