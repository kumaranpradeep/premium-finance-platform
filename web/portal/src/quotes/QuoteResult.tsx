import { formatDate, formatMoney } from '../format'
import type { Quote } from './types'

export function QuoteResult({ quote }: { quote: Quote }) {
  return (
    <section aria-label="Quote">
      <h2>Your quote</h2>
      <dl>
        <dt>Premium</dt>
        <dd>{formatMoney(quote.premium)}</dd>
        <dt>Credit charge</dt>
        <dd>{formatMoney(quote.creditCharge)}</dd>
        <dt>Total payable</dt>
        <dd>{formatMoney(quote.totalPayable)}</dd>
        <dt>APR</dt>
        <dd>{quote.apr.toFixed(1)}%</dd>
        <dt>Flat rate (per year)</dt>
        <dd>{quote.annualFlatRatePercent.toFixed(2)}%</dd>
      </dl>

      <h3>{quote.numberOfInstalments} monthly payments</h3>
      <table>
        <thead>
          <tr>
            <th>Payment</th>
            <th>Due</th>
            <th>Amount</th>
          </tr>
        </thead>
        <tbody>
          {quote.instalments.map((instalment) => (
            <tr key={instalment.number}>
              <td>{instalment.number}</td>
              <td>{formatDate(instalment.dueDate)}</td>
              <td>{formatMoney(instalment.amount)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </section>
  )
}