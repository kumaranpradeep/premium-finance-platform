import type { AuthResponse } from './types'

const REFRESH_TOKEN_KEY = 'pf.refreshToken'

// In memory only: gone on page reload, which is fine because it lasts 15 minutes and we can refresh.
let accessToken: string | null = null

// localStorage can throw (private browsing, storage disabled), so every access is guarded.
function readRefreshToken(): string | null {
  try {
    return localStorage.getItem(REFRESH_TOKEN_KEY)
  } catch {
    return null
  }
}

export const tokenStore = {
  getAccessToken: () => accessToken,
  getRefreshToken: readRefreshToken,

  save(response: AuthResponse) {
    accessToken = response.accessToken
    try {
      localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken)
    } catch {
      // Can't persist: the user stays signed in until they reload the page.
    }
  },

  clear() {
    accessToken = null
    try {
      localStorage.removeItem(REFRESH_TOKEN_KEY)
    } catch {
      // Nothing stored.
    }
  },
}