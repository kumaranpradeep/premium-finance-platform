using Microsoft.EntityFrameworkCore;
using PremiumFinance.Application.Pricing;
using PremiumFinance.Domain.Pricing;

namespace PremiumFinance.Infrastructure.Persistence.Repositories;

public class RateCardRepository : IRateCardRepository
{
    private readonly PremiumFinanceDbContext _db;

    public RateCardRepository(PremiumFinanceDbContext db) => _db = db;

    public Task<RateCardEntry?> FindInForceAsync(string schemeCode, int numberOfInstalments, DateOnly onDate,
                                                 CancellationToken cancellationToken) =>
        _db.RateCardEntries
            .AsNoTracking()
            .Where(r => r.SchemeCode == schemeCode
                     && r.NumberOfInstalments == numberOfInstalments
                     && r.EffectiveFrom <= onDate
                     && (r.EffectiveTo == null || r.EffectiveTo >= onDate))
            .OrderByDescending(r => r.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken);
}