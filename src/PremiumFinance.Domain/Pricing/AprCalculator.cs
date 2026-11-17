namespace PremiumFinance.Domain.Pricing;

// APR as defined by the Consumer Credit rules: the yearly rate at which the amount of credit
// equals the present value of all repayments, with time measured in years of 365 days.
public static class AprCalculator
{
    private const int Iterations = 200;
    private const double HighestRateTried = 10.0;   // 1,000% APR: far above anything we'd lend at

    public static decimal Calculate(decimal amountOfCredit, DateOnly startDate,
                                    IEnumerable<(DateOnly DueDate, decimal Amount)> repayments)
    {
        var flows = repayments
            .Select(r => (Years: (r.DueDate.DayNumber - startDate.DayNumber) / 365.0, Amount: (double)r.Amount))
            .ToList();

        // How much the repayments are worth today at a given yearly rate, minus what was lent.
        // Positive = the rate is too low; negative = too high.
        double Gap(double rate) =>
            flows.Sum(f => f.Amount / Math.Pow(1 + rate, f.Years)) - (double)amountOfCredit;

        double low = 0, high = HighestRateTried;
        for (int i = 0; i < Iterations; i++)
        {
            double middle = (low + high) / 2;
            if (Gap(middle) > 0) low = middle;
            else high = middle;
        }

        double apr = (low + high) / 2 * 100;
        return Math.Round((decimal)apr, 1, MidpointRounding.AwayFromZero);
    }
}