using PremiumFinance.Application.Pricing;
using PremiumFinance.Domain.Exceptions;
using PremiumFinance.Domain.Pricing;

namespace PremiumFinance.Application.Quotes;

public class QuoteService
{
    // Until brokers are linked to pricing schemes, every quote uses the standard scheme.
    private const string StandardScheme = "STANDARD";
    private const int MaxDaysAhead = 30;
    private static readonly TimeZoneInfo UkTime = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

    private readonly IRateCardRepository _rateCard;
    private readonly TimeProvider _clock;

    public QuoteService(IRateCardRepository rateCard, TimeProvider clock)
    {
        _rateCard = rateCard;
        _clock = clock;
    }

    public async Task<QuoteResponse> GetQuoteAsync(QuoteRequest request, CancellationToken cancellationToken)
    {
        DateOnly startDate = request.PolicyStartDate!.Value;
        DateOnly today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(_clock.GetUtcNow(), UkTime).DateTime);

        if (startDate < today)
            throw new BusinessRuleException("The policy start date can't be in the past.");
        if (startDate > today.AddDays(MaxDaysAhead))
            throw new BusinessRuleException($"Quotes are available for policies starting within the next {MaxDaysAhead} days.");

        int instalments = request.NumberOfInstalments!.Value;
        RateCardEntry rate = await _rateCard.FindInForceAsync(StandardScheme, instalments, startDate, cancellationToken)
            ?? throw new BusinessRuleException($"We don't offer {instalments} monthly instalments.");

        Quote quote = Quote.Create(request.Premium!.Value, rate, startDate);

        return new QuoteResponse(
            quote.Premium, quote.NumberOfInstalments, quote.AnnualFlatRatePercent, quote.CreditCharge,
            quote.TotalPayable, quote.Apr, quote.StartDate,
            quote.Instalments.Select(i => new QuoteInstalmentResponse(i.Number, i.DueDate, i.Amount)).ToList());
    }
}