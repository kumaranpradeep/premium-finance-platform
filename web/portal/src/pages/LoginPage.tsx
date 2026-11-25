import { useState, type FormEvent } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useMutation } from '@tanstack/react-query'
import { useAuth } from '../auth/useAuth'
import { ErrorMessage } from '../components/ErrorMessage'

export function LoginPage() {
  const { state, signIn } = useAuth()
  const location = useLocation()
  const from = (location.state as { from?: string } | null)?.from ?? '/'

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const login = useMutation({ mutationFn: () => signIn(email, password) })

  if (state.status === 'loading') return <p>Loading…</p>
  // Signed in (just now, or already): go to the page they wanted.
  if (state.status === 'signedIn') return <Navigate to={from} replace />

  function handleSubmit(event: FormEvent) {
    event.preventDefault()
    login.mutate()
  }

  return (
    <main>
      <h1>Sign in to Premium Finance</h1>

      <form onSubmit={handleSubmit}>
        <label>
          Email
          <input type="email" required autoComplete="username"
                 value={email} onChange={(e) => setEmail(e.target.value)} />
        </label>

        <label>
          Password
          <input type="password" required autoComplete="current-password"
                 value={password} onChange={(e) => setPassword(e.target.value)} />
        </label>

        <button type="submit" disabled={login.isPending}>
          {login.isPending ? 'Signing in…' : 'Sign in'}
        </button>
      </form>

      {login.isError && <ErrorMessage error={login.error} />}
    </main>
  )
}