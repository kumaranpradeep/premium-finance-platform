import { useAuth } from '../auth/useAuth'
import { ErrorMessage } from '../components/ErrorMessage'
import { formatTimestamp } from '../format'
import { CreateUserForm } from '../users/CreateUserForm'
import { useSetUserActive, useUsers } from '../users/useUsers'

export function UsersPage() {
  const { state } = useAuth()
  const currentUserId = state.status === 'signedIn' ? state.user.id : null
  const users = useUsers()
  const setActive = useSetUserActive()

  return (
    <>
      <h1>Users</h1>
      <CreateUserForm />

      <h2>All users</h2>
      {users.isPending && <p>Loading users…</p>}
      {users.isError && <ErrorMessage error={users.error} />}
      {setActive.isError && <ErrorMessage error={setActive.error} />}

      {users.data && (
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Role</th>
              <th>Status</th>
              <th>Created</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {users.data.map((user) => (
              <tr key={user.id}>
                <td>{user.fullName}</td>
                <td>{user.email}</td>
                <td>{user.roles.join(', ')}</td>
                <td>{!user.isActive ? 'Deactivated' : user.isLockedOut ? 'Locked' : 'Active'}</td>
                <td>{formatTimestamp(user.createdOnUtc)}</td>
                <td>
                  {/* The API refuses to deactivate yourself; don't offer the button either. */}
                  {user.id !== currentUserId && (
                    <button type="button" disabled={setActive.isPending}
                            onClick={() => setActive.mutate({ id: user.id, active: !user.isActive })}>
                      {user.isActive ? 'Deactivate' : 'Reactivate'}
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </>
  )
}