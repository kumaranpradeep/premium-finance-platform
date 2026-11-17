import { useMutation } from '@tanstack/react-query'
import { apiRequest } from '../api/client'
import type { Quote, QuoteRequest } from './types'

export function useGetQuote() {
  return useMutation({
    mutationFn: (request: QuoteRequest) =>
      apiRequest<Quote>('/api/quotes', { method: 'POST', body: JSON.stringify(request) }),
  })
}