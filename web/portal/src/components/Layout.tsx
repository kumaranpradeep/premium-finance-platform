import { Link, Outlet } from 'react-router-dom'

export function Layout() {
  return (
    <>
      <header>
        <Link to="/">Premium Finance</Link>
        <nav>
          <Link to="/quotes">Get a quote</Link>
        </nav>
      </header>
      <main>
        <Outlet />
      </main>
    </>
  )
}