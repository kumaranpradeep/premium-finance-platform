import { useState, type FormEvent } from 'react'
import { ErrorMessage } from '../components/ErrorMessage'
import { QuoteResult } from '../quotes/QuoteResult'
import { useGetQuote } from '../quotes/useGetQuote'

// The terms on the STANDARD rate card. When brokers have their own schemes, these come from the API.
const OFFERED_INSTALMENTS = [10, 11, 12]

export function QuotePage() {
  const today = new Date().toLocaleDateString('en-CA') // 'YYYY-MM-DD' in the user's local time
  const [premium, setPremium] = useState('')
  const [instalments, setInstalments] = useState('10')
  const [startDate, setStartDate] = useState(today)
  const getQuote = useGetQuote()

  // Any change to the inputs clears the old quote, so the broker never reads a quote for different numbers.
  function change(setValue: (value: string) => void, value: string) {
    setValue(value)
    getQuote.reset()
  }

  function handleSubmit(event: FormEvent) {
    event.preventDefault()
    getQuote.mutate({
      premium: Number(premium),
      numberOfInstalments: Number(instalments),
      policyStartDate: startDate,
    })
  }

  return (
    <>
      <h1>Get a quote</h1>

      <form onSubmit={handleSubmit}>
        <label>
          Premium (£)
          <input type="number" min="0.01" step="0.01" required
                 value={premium} onChange={(e) => change(setPremium, e.target.value)} />
        </label>

        <label>
          Monthly instalments
          <select value={instalments} onChange={(e) => change(setInstalments, e.target.value)}>
            {OFFERED_INSTALMENTS.map((n) => (
              <option key={n} value={n}>{n}</option>
            ))}
          </select>
        </label>

        <label>
          Policy start date
          <input type="date" required min={today}
                 value={startDate} onChange={(e) => change(setStartDate, e.target.value)} />
        </label>

        <button type="submit" disabled={getQuote.isPending}>
          {getQuote.isPending ? 'Getting quote…' : 'Get quote'}
        </button>
      </form>

      {getQuote.isError && <ErrorMessage error={getQuote.error} />}
      {getQuote.data && <QuoteResult quote={getQuote.data} />}
    </>
  )
}