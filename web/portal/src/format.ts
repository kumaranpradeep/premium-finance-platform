const money = new Intl.NumberFormat('en-GB', { style: 'currency', currency: 'GBP' })

export const formatMoney = (amount: number) => money.format(amount)

// 'YYYY-MM-DD' → '20 Dec 2026'. Adding a time makes the browser read it as a local date,
// not midnight UTC (which shows the previous day in some time zones).
export const formatDate = (isoDate: string) =>
  new Date(`${isoDate}T00:00:00`).toLocaleDateString('en-GB', { day: 'numeric', month: 'short', year: 'numeric' })