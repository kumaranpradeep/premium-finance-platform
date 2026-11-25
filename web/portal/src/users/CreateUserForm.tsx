import { useState, type FormEvent } from 'react'
import { Roles, type Role } from '../auth/types'
import { ErrorMessage } from '../components/ErrorMessage'
import { useCreateUser } from './useUsers'

const ROLE_OPTIONS: Role[] = [
  Roles.Broker, Roles.Agent, Roles.TeamLeader, Roles.Underwriter, Roles.Operations, Roles.Compliance, Roles.Admin,
]

export function CreateUserForm() {
  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [role, setRole] = useState<Role>(Roles.Broker)
  const [temporaryPassword, setTemporaryPassword] = useState('')
  const createUser = useCreateUser()

  function handleSubmit(event: FormEvent) {
    event.preventDefault()
    createUser.mutate(
      { fullName, email, role, temporaryPassword },
      {
        // Clear the form only when it worked; on an error, keep what was typed so it can be fixed.
        onSuccess: () => {
          setFullName('')
          setEmail('')
          setTemporaryPassword('')
        },
      },
    )
  }

  return (
    <>
      <h2>Create a user</h2>
      <form onSubmit={handleSubmit}>
        <label>
          Full name
          <input required maxLength={200} value={fullName} onChange={(e) => setFullName(e.target.value)} />
        </label>

        <label>
          Email
          <input type="email" required value={email} onChange={(e) => setEmail(e.target.value)} />
        </label>

        <label>
          Role
          <select value={role} onChange={(e) => setRole(e.target.value as Role)}>
            {ROLE_OPTIONS.map((option) => (
              <option key={option} value={option}>{option}</option>
            ))}
          </select>
        </label>

        <label>
          Temporary password (at least 12 characters, with upper case, lower case and a number)
          <input type="password" required minLength={12} autoComplete="new-password"
                 value={temporaryPassword} onChange={(e) => setTemporaryPassword(e.target.value)} />
        </label>

        <button type="submit" disabled={createUser.isPending}>
          {createUser.isPending ? 'Creating…' : 'Create user'}
        </button>
      </form>

      {createUser.isError && <ErrorMessage error={createUser.error} />}
      {createUser.isSuccess && <p>Created {createUser.data.fullName} ({createUser.data.roles.join(', ')}).</p>}
    </>
  )
}