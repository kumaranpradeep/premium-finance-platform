import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiRequest } from '../api/client'
import type { CreateUserRequest, UserSummary } from './types'

const usersKey = ['users'] as const

export function useUsers() {
  return useQuery({
    queryKey: usersKey,
    queryFn: () => apiRequest<UserSummary[]>('/api/users'),
  })
}

export function useCreateUser() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (request: CreateUserRequest) =>
      apiRequest<UserSummary>('/api/users', { method: 'POST', body: JSON.stringify(request) }),
    // The list on screen is now out of date: mark it stale so React Query fetches it again.
    onSuccess: () => queryClient.invalidateQueries({ queryKey: usersKey }),
  })
}

export function useSetUserActive() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, active }: { id: string; active: boolean }) =>
      apiRequest<void>(`/api/users/${encodeURIComponent(id)}/${active ? 'reactivate' : 'deactivate'}`, {
        method: 'POST',
      }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: usersKey }),
  })
}