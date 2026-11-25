import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { ForbiddenPage } from '../pages/ForbiddenPage'
import type { Role } from './types'
import { useAuth } from './useAuth'

// Wraps routes: signed-out users go to sign-in; signed-in users without the role see "not allowed".
export function RequireAuth({ roles }: { roles?: Role[] }) {
  const { state, hasAnyRole } = useAuth()
  const location = useLocation()

  if (state.status === 'loading') return <p>Loading…</p>

  if (state.status === 'signedOut') {
    // Remember where they were going, so sign-in can send them back there.
    return <Navigate to="/login" replace state={{ from: location.pathname }} />
  }

  if (roles && !hasAnyRole(...roles)) return <ForbiddenPage />

  return <Outlet />
}