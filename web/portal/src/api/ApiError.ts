// One error type for every failed API call, built from the API's ProblemDetails response.
export class ApiError extends Error {
  readonly status: number
  readonly title: string
  readonly traceId: string | null

  constructor(status: number, title: string, detail: string, traceId: string | null) {
    super(detail)
    this.name = 'ApiError'
    this.status = status
    this.title = title
    this.traceId = traceId
  }
}