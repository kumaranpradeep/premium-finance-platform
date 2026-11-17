using PremiumFinance.Domain.Pricing;

namespace PremiumFinance.Application.Pricing;

public interface IRateCardRepository
{
    // The rate in force on a date for a scheme and number of instalments, or null if we don't offer it.
    Task<RateCardEntry?> FindInForceAsync(string schemeCode, int numberOfInstalments, DateOnly onDate,
                                          CancellationToken cancellationToken);
}