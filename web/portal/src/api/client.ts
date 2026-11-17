import { config } from '../config'
import { ApiError } from './ApiError'

type ProblemDetails = {
  status?: number
  title?: string
  detail?: string
  traceId?: string
  errors?: Record<string, string[]>
}

export async function apiRequest<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers)
  headers.set('Accept', 'application/json')
  if (init.body) headers.set('Content-Type', 'application/json')
  // One ID per call, so a failed request can be found in the API's logs.
  headers.set('X-Correlation-Id', crypto.randomUUID())

  let response: Response
  try {
    response = await fetch(`${config.apiBaseUrl}${path}`, { ...init, headers })
  } catch {
    // fetch only throws when there's no response at all: API down, no network, or blocked by CORS.
    throw new ApiError(0, 'Service unavailable', 'Could not reach the server. Please try again.', null)
  }

  if (!response.ok) {
    const problem: ProblemDetails = await response.json().catch(() => ({}))
    const validationMessages = problem.errors ? Object.values(problem.errors).flat().join(' ') : undefined
    throw new ApiError(
      response.status,
      problem.title ?? response.statusText,
      problem.detail ?? validationMessages ?? 'Something went wrong. Please try again.',
      problem.traceId ?? response.headers.get('X-Correlation-Id'),
    )
  }

  if (response.status === 204) return undefined as T
  return (await response.json()) as T
}