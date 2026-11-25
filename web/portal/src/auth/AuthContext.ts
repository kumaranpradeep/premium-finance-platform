import { createContext } from 'react'
import type { AuthUser, Role } from './types'

export type AuthState =
  | { status: 'loading' }                       // checking for a saved session on page load
  | { status: 'signedOut' }
  | { status: 'signedIn'; user: AuthUser }

export type AuthContextValue = {
  state: AuthState
  signIn: (email: string, password: string) => Promise<void>
  signOut: () => Promise<void>
  hasAnyRole: (...roles: Role[]) => boolean
}

export const AuthContext = createContext<AuthContextValue | null>(null)