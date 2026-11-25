import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { ApiError } from '../api/ApiError'
import { apiRequest, refreshSession, setSessionExpiredHandler } from '../api/client'
import { AuthContext, type AuthContextValue, type AuthState } from './AuthContext'
import { tokenStore } from './tokenStore'
import type { AuthResponse, Role } from './types'

export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()

  // With a saved refresh token we don't know yet if the session is still valid: start as 'loading'.
  const [state, setState] = useState<AuthState>(() =>
    tokenStore.getRefreshToken() ? { status: 'loading' } : { status: 'signedOut' },
  )

  // When the client gives up on the session (refresh rejected), sign out here too.
  useEffect(() => {
    setSessionExpiredHandler(() => {
      queryClient.clear()
      setState({ status: 'signedOut' })
    })
  }, [queryClient])

  // Page load with a saved refresh token: get a new access token and the user in one call.
  useEffect(() => {
    if (!tokenStore.getRefreshToken()) return

    let cancelled = false
    refreshSession()
      .then((response) => {
        if (!cancelled) setState({ status: 'signedIn', user: response.user })
      })
      .catch((error) => {
        // Only forget the token if the API rejected it; if the API was just unreachable, keep it.
        if (error instanceof ApiError && error.status === 401) tokenStore.clear()
        if (!cancelled) setState({ status: 'signedOut' })
      })

    return () => {
      cancelled = true
    }
  }, [])

  const signIn = useCallback(async (email: string, password: string) => {
    const response = await apiRequest<AuthResponse>(
      '/api/auth/login',
      { method: 'POST', body: JSON.stringify({ email, password }) },
      { anonymous: true },
    )
    tokenStore.save(response)
    setState({ status: 'signedIn', user: response.user })
  }, [])

  const signOut = useCallback(async () => {
    const refreshToken = tokenStore.getRefreshToken()

    // Sign out locally first, so the screen changes immediately even if the API is slow.
    tokenStore.clear()
    // Drop every cached API response: the next user on this computer must not see this user's data.
    queryClient.clear()
    setState({ status: 'signedOut' })

    if (refreshToken) {
      await apiRequest<void>(
        '/api/auth/logout',
        { method: 'POST', body: JSON.stringify({ refreshToken }) },
        { anonymous: true },
      ).catch(() => {
        // Already signed out locally; the token will expire on its own.
      })
    }
  }, [queryClient])

  const value = useMemo<AuthContextValue>(
    () => ({
      state,
      signIn,
      signOut,
      hasAnyRole: (...roles: Role[]) =>
        state.status === 'signedIn' && state.user.roles.some((role) => roles.includes(role)),
    }),
    [state, signIn, signOut],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}