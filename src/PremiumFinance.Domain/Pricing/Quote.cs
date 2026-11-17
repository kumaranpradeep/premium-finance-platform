using PremiumFinance.Domain.Exceptions;

namespace PremiumFinance.Domain.Pricing;

public record QuoteInstalment(int Number, DateOnly DueDate, decimal Amount);

// A priced quote: what the customer pays, and when. Nothing is saved; a quote becomes an agreement later.
public class Quote
{
    public const decimal MinimumPremium = 100m;
    public const decimal MaximumPremium = 50_000m;
    private const int LatestCollectionDay = 28;

    public decimal Premium { get; }
    public string SchemeCode { get; }
    public int NumberOfInstalments { get; }
    public decimal AnnualFlatRatePercent { get; }
    public DateOnly StartDate { get; }
    public decimal CreditCharge { get; }
    public decimal TotalPayable { get; }
    public decimal Apr { get; }
    public IReadOnlyList<QuoteInstalment> Instalments { get; }

    private Quote(decimal premium, RateCardEntry rate, DateOnly startDate, decimal creditCharge,
                  IReadOnlyList<QuoteInstalment> instalments)
    {
        Premium = premium;
        SchemeCode = rate.SchemeCode;
        NumberOfInstalments = rate.NumberOfInstalments;
        AnnualFlatRatePercent = rate.AnnualFlatRatePercent;
        StartDate = startDate;
        CreditCharge = creditCharge;
        TotalPayable = premium + creditCharge;
        Instalments = instalments;
        Apr = AprCalculator.Calculate(premium, startDate, instalments.Select(i => (i.DueDate, i.Amount)));
    }

    public static Quote Create(decimal premium, RateCardEntry rate, DateOnly startDate)
    {
        if (premium < MinimumPremium || premium > MaximumPremium)
            throw new BusinessRuleException(
                $"We can finance premiums from £{MinimumPremium:N0} to £{MaximumPremium:N0}.");

        // The caller must pick the rate in force on the start date; anything else is a bug, not a user error.
        if (!rate.IsEffectiveOn(startDate))
            throw new InvalidOperationException($"Rate {rate.Id} is not in force on {startDate:yyyy-MM-dd}.");

        int count = rate.NumberOfInstalments;

        decimal creditCharge = Math.Round(
            premium * rate.AnnualFlatRatePercent / 100m * count / 12m,
            2, MidpointRounding.AwayFromZero);

        decimal totalPayable = premium + creditCharge;

        // Round each instalment down to the penny; the leftover pennies go on the first one,
        // so the instalments always add up exactly to the total payable.
        decimal regular = Math.Floor(totalPayable / count * 100m) / 100m;
        decimal first = totalPayable - regular * (count - 1);

        int collectionDay = Math.Min(startDate.Day, LatestCollectionDay);
        DateOnly firstMonth = startDate.AddMonths(1);

        var instalments = new List<QuoteInstalment>();
        for (int number = 1; number <= count; number++)
        {
            DateOnly month = firstMonth.AddMonths(number - 1);
            var dueDate = new DateOnly(month.Year, month.Month, collectionDay);
            instalments.Add(new QuoteInstalment(number, dueDate, number == 1 ? first : regular));
        }

        return new Quote(premium, rate, startDate, creditCharge, instalments);
    }
}