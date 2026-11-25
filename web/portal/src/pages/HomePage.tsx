import { useAuth } from '../auth/useAuth'

export function HomePage() {
  const { state } = useAuth()
  const name = state.status === 'signedIn' ? state.user.fullName : ''
  return <h1>Welcome, {name}</h1>
}