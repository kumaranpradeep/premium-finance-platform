import { Link } from 'react-router-dom'

export function NotFoundPage() {
  return (
    <>
      <h1>Page not found</h1>
      <p>
        The page you're looking for doesn't exist. <Link to="/">Go to the home page</Link>
      </p>
    </>
  )
}