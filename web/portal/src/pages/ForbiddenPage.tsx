import { Link } from 'react-router-dom'

export function ForbiddenPage() {
  return (
    <>
      <h1>Not allowed</h1>
      <p>
        Your role doesn't have access to this page. <Link to="/">Go to the home page</Link>
      </p>
    </>
  )
}