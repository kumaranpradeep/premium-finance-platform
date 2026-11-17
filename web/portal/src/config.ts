const apiBaseUrl = import.meta.env.VITE_API_BASE_URL

if (!apiBaseUrl) {
  // Fail loudly at startup rather than with confusing errors on the first API call.
  throw new Error('VITE_API_BASE_URL is not set. Add it to .env.development (local) or the pipeline (Azure).')
}

export const config = { apiBaseUrl }