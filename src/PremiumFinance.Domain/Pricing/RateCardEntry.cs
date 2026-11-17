using PremiumFinance.Domain.Common;
using PremiumFinance.Domain.Exceptions;

namespace PremiumFinance.Domain.Pricing;

// One rate on the rate card: the yearly flat rate for a scheme and number of instalments, from a given date.
public class RateCardEntry : AuditableEntity
{
    public int Id { get; private set; }
    public string SchemeCode { get; private set; } = string.Empty;
    public int NumberOfInstalments { get; private set; }
    public decimal AnnualFlatRatePercent { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }   // last day the rate applies; null = until replaced

    private RateCardEntry() { }   // for EF Core

    public RateCardEntry(string schemeCode, int numberOfInstalments, decimal annualFlatRatePercent,
                         DateOnly effectiveFrom, DateOnly? effectiveTo = null)
    {
        if (string.IsNullOrWhiteSpace(schemeCode))
            throw new BusinessRuleException("A rate must belong to a pricing scheme.");
        if (numberOfInstalments is < 1 or > 12)
            throw new BusinessRuleException("Agreements run for 1 to 12 monthly instalments.");
        if (annualFlatRatePercent is <= 0 or > 50)
            throw new BusinessRuleException("The flat rate must be above 0% and no more than 50%.");
        if (effectiveTo is not null && effectiveTo < effectiveFrom)
            throw new BusinessRuleException("A rate can't end before it starts.");

        SchemeCode = schemeCode;
        NumberOfInstalments = numberOfInstalments;
        AnnualFlatRatePercent = annualFlatRatePercent;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
    }

    public bool IsEffectiveOn(DateOnly date) =>
        EffectiveFrom <= date && (EffectiveTo is null || date <= EffectiveTo);
}