import { Link, Outlet } from 'react-router-dom'

export function Layout() {
  return (
    <>
      <header>
        <Link to="/">Premium Finance</Link>
      </header>
      <main>
        <Outlet />
      </main>
    </>
  )
}