// Same names as the API's Roles class.
export const Roles = {
  Admin: 'Admin',
  Broker: 'Broker',
  Agent: 'Agent',
  TeamLeader: 'TeamLeader',
  Underwriter: 'Underwriter',
  Operations: 'Operations',
  Compliance: 'Compliance',
} as const

export type Role = (typeof Roles)[keyof typeof Roles]

// Same shapes as the API's AuthUser and AuthResponse.
export type AuthUser = {
  id: string
  email: string
  fullName: string
  roles: Role[]
}

export type AuthResponse = {
  accessToken: string
  accessTokenExpiresOnUtc: string
  refreshToken: string
  refreshTokenExpiresOnUtc: string
  user: AuthUser
}