namespace PremiumFinance.Domain.Brokers;

public enum CommissionDealType
{
    // A % of the credit charge (the interest we earn), e.g. 40% of £95 = £38.
    ShareOfCreditCharge,

    // A % of the premium financed, e.g. 2% of £1,200 = £24.
    PercentOfAmountFinanced
}