using PremiumFinance.Domain.Exceptions;
using PremiumFinance.Domain.Pricing;

namespace PremiumFinance.UnitTests.Domain;

public class QuoteTests
{
    private static readonly RateCardEntry TenAt950 = new("STANDARD", 10, 9.50m, new DateOnly(2026, 1, 1));
    private static readonly RateCardEntry ElevenAt975 = new("STANDARD", 11, 9.75m, new DateOnly(2026, 1, 1));
    private static readonly RateCardEntry TwelveAt990 = new("STANDARD", 12, 9.90m, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));

    [Fact]
    public void Ten_instalments_that_divide_exactly()
    {
        // 1,200 × 9.5% × 10/12 = 95.00 → total 1,295.00 → 10 × 129.50
        var quote = Quote.Create(1200m, TenAt950, new DateOnly(2026, 11, 17));

        Assert.Equal(95.00m, quote.CreditCharge);
        Assert.Equal(1295.00m, quote.TotalPayable);
        Assert.All(quote.Instalments, i => Assert.Equal(129.50m, i.Amount));
    }

    [Fact]
    public void Leftover_pennies_go_on_the_first_instalment()
    {
        // 1,000 × 9.9% × 12/12 = 99.00 → total 1,099.00 → 1,099 / 12 = 91.583…
        // regular 91.58 × 11 = 1,007.38 → first = 1,099.00 − 1,007.38 = 91.62
        var quote = Quote.Create(1000m, TwelveAt990, new DateOnly(2026, 11, 17));

        Assert.Equal(91.62m, quote.Instalments[0].Amount);
        Assert.All(quote.Instalments.Skip(1), i => Assert.Equal(91.58m, i.Amount));
        Assert.Equal(quote.TotalPayable, quote.Instalments.Sum(i => i.Amount));
    }

    [Fact]
    public void Credit_charge_is_rounded_to_the_penny()
    {
        // 850 × 9.75% × 11/12 = 75.96875 → 75.97; total 925.97 → regular 84.17, first 84.27
        var quote = Quote.Create(850m, ElevenAt975, new DateOnly(2026, 11, 17));

        Assert.Equal(75.97m, quote.CreditCharge);
        Assert.Equal(84.27m, quote.Instalments[0].Amount);
        Assert.Equal(84.17m, quote.Instalments[1].Amount);
        Assert.Equal(925.97m, quote.Instalments.Sum(i => i.Amount));
    }

    [Fact]
    public void Due_dates_are_monthly_from_one_month_after_the_start()
    {
        var quote = Quote.Create(1000m, TwelveAt990, new DateOnly(2026, 11, 17));

        Assert.Equal(new DateOnly(2026, 12, 17), quote.Instalments[0].DueDate);
        Assert.Equal(new DateOnly(2027, 1, 17), quote.Instalments[1].DueDate);
        Assert.Equal(new DateOnly(2027, 11, 17), quote.Instalments[11].DueDate);
    }

    [Fact]
    public void Start_on_the_31st_collects_on_the_28th()
    {
        var quote = Quote.Create(1200m, TenAt950, new DateOnly(2026, 1, 31));

        Assert.Equal(new DateOnly(2026, 2, 28), quote.Instalments[0].DueDate);
        Assert.Equal(new DateOnly(2026, 3, 28), quote.Instalments[1].DueDate);
    }

    [Fact]
    public void Premium_below_the_minimum_is_rejected()
    {
        var ex = Assert.Throws<BusinessRuleException>(() =>
            Quote.Create(50m, TenAt950, new DateOnly(2026, 11, 17)));

        Assert.Equal("We can finance premiums from £100 to £50,000.", ex.Message);
    }
        [Fact]
    public void Apr_is_shown_alongside_the_flat_rate()
    {
        // 9.5% flat over 10 months, but the customer repays monthly: APR 18.4%
        var quote = Quote.Create(1200m, TenAt950, new DateOnly(2026, 11, 17));

        Assert.Equal(18.4m, quote.Apr);
    }

    [Theory]
    [InlineData(1000, 12, 19.4)]
    [InlineData(850, 11, 19.0)]
    public void Apr_for_other_terms(decimal premium, int instalments, decimal expectedApr)
    {
        var rate = instalments == 12 ? TwelveAt990 : ElevenAt975;

        var quote = Quote.Create(premium, rate, new DateOnly(2026, 11, 17));

        Assert.Equal(expectedApr, quote.Apr);
    }

    [Fact]
    public void Collecting_earlier_in_the_month_raises_the_apr()
    {
        // Same price as the 18.4% case, but starting on the 31st means collections on the 28th:
        // each payment is a few days earlier, so the customer borrows for slightly less time.
        var quote = Quote.Create(1200m, TenAt950, new DateOnly(2026, 1, 31));

        Assert.Equal(18.8m, quote.Apr);
    }
}