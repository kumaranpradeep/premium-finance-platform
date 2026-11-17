import { ApiError } from '../api/ApiError'

// Shows an API error the same way on every screen, with the reference support needs.
export function ErrorMessage({ error }: { error: unknown }) {
  const message = error instanceof Error ? error.message : 'Something went wrong.'
  const reference = error instanceof ApiError ? error.traceId : null

  return (
    <div role="alert">
      <p>{message}</p>
      {reference && <p>Reference: {reference}</p>}
    </div>
  )
}