import type { Role } from '../auth/types'

// Same shapes as the API's UserSummary and CreateUserRequest.
export type UserSummary = {
  id: string
  email: string
  fullName: string
  roles: Role[]
  isActive: boolean
  isLockedOut: boolean
  createdOnUtc: string
}

export type CreateUserRequest = {
  fullName: string
  email: string
  role: Role
  temporaryPassword: string
}