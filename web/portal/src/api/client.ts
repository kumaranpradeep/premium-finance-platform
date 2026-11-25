import { config } from '../config'
import { tokenStore } from '../auth/tokenStore'
import type { AuthResponse } from '../auth/types'
import { ApiError } from './ApiError'

type ProblemDetails = {
  status?: number
  title?: string
  detail?: string
  traceId?: string
  errors?: Record<string, string[]>
}

type RequestOptions = {
  // Login, refresh and logout: no token, and a 401 must not trigger a refresh.
  anonymous?: boolean
}

// AuthProvider sets this, so the client can tell the app "the session is over" when a refresh fails.
let onSessionExpired: () => void = () => {}
export function setSessionExpiredHandler(handler: () => void) {
  onSessionExpired = handler
}

// One HTTP call: headers, fetch, and turning errors into ApiError. No auth logic.
async function send<T>(path: string, init: RequestInit, accessToken: string | null): Promise<T> {
  const headers = new Headers(init.headers)
  headers.set('Accept', 'application/json')
  if (init.body) headers.set('Content-Type', 'application/json')
  // One ID per call, so a failed request can be found in the API's logs.
  headers.set('X-Correlation-Id', crypto.randomUUID())
  if (accessToken) headers.set('Authorization', `Bearer ${accessToken}`)

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

// The refresh currently in progress, shared by everyone who needs it.
let refreshInFlight: Promise<AuthResponse> | null = null

export function refreshSession(): Promise<AuthResponse> {
  if (!refreshInFlight) {
    const refreshToken = tokenStore.getRefreshToken()

    refreshInFlight = (
      refreshToken
        ? send<AuthResponse>('/api/auth/refresh', { method: 'POST', body: JSON.stringify({ refreshToken }) }, null)
        : Promise.reject(new ApiError(401, 'Unauthorized', 'Please sign in.', null))
    )
      .then((response) => {
        tokenStore.save(response)
        return response
      })
      .finally(() => {
        refreshInFlight = null
      })
  }
  return refreshInFlight
}

export async function apiRequest<T>(path: string, init: RequestInit = {}, options: RequestOptions = {}): Promise<T> {
  if (options.anonymous) return send<T>(path, init, null)

  try {
    return await send<T>(path, init, tokenStore.getAccessToken())
  } catch (error) {
    // Only a 401 means "your token is missing or expired". Everything else goes straight to the caller.
    if (!(error instanceof ApiError) || error.status !== 401) throw error

    try {
      await refreshSession()
    } catch (refreshError) {
      // The refresh token itself was rejected: the session is really over.
      if (refreshError instanceof ApiError && refreshError.status === 401) {
        tokenStore.clear()
        onSessionExpired()
        throw new ApiError(401, 'Unauthorized', 'Your session has ended. Please sign in again.', null)
      }
      // Network or server problem during the refresh: report it, but keep the session.
      throw refreshError
    }

    // Same request again, now with the new access token. If this fails too, the error goes to the caller.
    return send<T>(path, init, tokenStore.getAccessToken())
  }
}