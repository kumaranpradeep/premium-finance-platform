import { Route, Routes } from 'react-router-dom'
import { RequireAuth } from './auth/RequireAuth'
import { Roles } from './auth/types'
import { Layout } from './components/Layout'
import { HomePage } from './pages/HomePage'
import { LoginPage } from './pages/LoginPage'
import { NotFoundPage } from './pages/NotFoundPage'
import { QuotePage } from './pages/QuotePage'
import { UsersPage } from './pages/UsersPage'

function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      {/* Everything else needs a signed-in user. */}
      <Route element={<RequireAuth />}>
        <Route element={<Layout />}>
          <Route path="/" element={<HomePage />} />

          <Route element={<RequireAuth roles={[Roles.Broker, Roles.Agent, Roles.TeamLeader]} />}>
            <Route path="/quotes" element={<QuotePage />} />
          </Route>

          <Route element={<RequireAuth roles={[Roles.Admin]} />}>
            <Route path="/users" element={<UsersPage />} />
          </Route>

          <Route path="*" element={<NotFoundPage />} />
        </Route>
      </Route>
    </Routes>
  )
}

export default App