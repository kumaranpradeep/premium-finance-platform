// The same shapes as the API's QuoteRequest and QuoteResponse.
export type QuoteRequest = {
  premium: number
  numberOfInstalments: number
  policyStartDate: string // 'YYYY-MM-DD'
}

export type QuoteInstalment = {
  number: number
  dueDate: string
  amount: number
}

export type Quote = {
  premium: number
  numberOfInstalments: number
  annualFlatRatePercent: number
  creditCharge: number
  totalPayable: number
  apr: number
  policyStartDate: string
  instalments: QuoteInstalment[]
}