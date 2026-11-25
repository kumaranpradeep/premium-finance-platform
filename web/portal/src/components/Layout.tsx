import { Link, Outlet } from 'react-router-dom'
import { Roles } from '../auth/types'
import { useAuth } from '../auth/useAuth'

export function Layout() {
  const { state, signOut, hasAnyRole } = useAuth()
  const user = state.status === 'signedIn' ? state.user : null

  return (
    <>
      <header>
        <Link to="/">Premium Finance</Link>
        <nav>
          {/* Only show links the user's role can use; the API enforces the same rules. */}
          {hasAnyRole(Roles.Broker, Roles.Agent, Roles.TeamLeader) && <Link to="/quotes">Get a quote</Link>}
          {hasAnyRole(Roles.Admin) && <Link to="/users">Users</Link>}
        </nav>
        {user && (
          <div className="user">
            <span>{user.fullName} ({user.roles.join(', ')})</span>
            <button type="button" onClick={() => void signOut()}>Sign out</button>
          </div>
        )}
      </header>
      <main>
        <Outlet />
      </main>
    </>
  )
}