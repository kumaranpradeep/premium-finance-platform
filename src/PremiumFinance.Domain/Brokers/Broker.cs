using PremiumFinance.Domain.Common;
using PremiumFinance.Domain.Exceptions;

namespace PremiumFinance.Domain.Brokers;

// An insurance broker firm that introduces customers to us, and the commission deal agreed with it.
public class Broker : AuditableEntity
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    // The FCA's Firm Reference Number: identifies the firm on the FCA register. Never changes.
    public string FcaReferenceNumber { get; private set; } = string.Empty;

    public string ContactEmail { get; private set; } = string.Empty;
    public BrokerStatus Status { get; private set; }
    public string? SuspensionReason { get; private set; }

    public CommissionDealType CommissionDealType { get; private set; }
    public decimal CommissionRatePercent { get; private set; }

    // Agreements ending within this many months of starting have their commission clawed back.
    public int ClawbackWindowMonths { get; private set; }

    // Changed by SQL Server on every update; used to detect two people editing the same broker.
    public byte[] RowVersion { get; private set; } = [];

    private Broker() { }   // for EF Core

    public Broker(string name, string fcaReferenceNumber, string contactEmail,
                  CommissionDealType dealType, decimal ratePercent, int clawbackWindowMonths)
    {
        string frn = fcaReferenceNumber?.Trim() ?? string.Empty;
        if (frn.Length is < 6 or > 7 || !frn.All(char.IsAsciiDigit))
            throw new BusinessRuleException("The FCA Firm Reference Number must be 6 or 7 digits.");

        FcaReferenceNumber = frn;
        UpdateDetails(name, contactEmail);
        ChangeCommissionDeal(dealType, ratePercent, clawbackWindowMonths);
        Status = BrokerStatus.Active;
    }

    public void UpdateDetails(string name, string contactEmail)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 200)
            throw new BusinessRuleException("The broker's name is required and can be at most 200 characters.");
        if (string.IsNullOrWhiteSpace(contactEmail) || contactEmail.Trim().Length > 256 || !contactEmail.Contains('@'))
            throw new BusinessRuleException("A valid contact email is required.");

        Name = name.Trim();
        ContactEmail = contactEmail.Trim();
    }

    // Applies to agreements created from now on; existing agreements keep the deal they were written under.
    public void ChangeCommissionDeal(CommissionDealType dealType, decimal ratePercent, int clawbackWindowMonths)
    {
        // Upper limits agreed by the commercial team: a typo like 400% instead of 40% is rejected.
        decimal maxRate = dealType switch
        {
            CommissionDealType.ShareOfCreditCharge => 60m,
            CommissionDealType.PercentOfAmountFinanced => 5m,
            _ => throw new BusinessRuleException("Unknown commission deal type.")
        };

        if (ratePercent <= 0 || ratePercent > maxRate)
            throw new BusinessRuleException($"For this deal type the commission rate must be above 0% and no more than {maxRate}%.");
        if (decimal.Round(ratePercent, 2) != ratePercent)
            throw new BusinessRuleException("The commission rate can have at most 2 decimal places.");
        if (clawbackWindowMonths is < 0 or > 12)
            throw new BusinessRuleException("The clawback window must be between 0 and 12 months.");

        CommissionDealType = dealType;
        CommissionRatePercent = ratePercent;
        ClawbackWindowMonths = clawbackWindowMonths;
    }

    public void Suspend(string reason)
    {
        if (Status == BrokerStatus.Suspended)
            throw new BusinessRuleException($"{Name} is already suspended.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessRuleException("A reason is required to suspend a broker.");

        Status = BrokerStatus.Suspended;
        SuspensionReason = reason.Trim();
    }

    public void Reinstate()
    {
        if (Status == BrokerStatus.Active)
            throw new BusinessRuleException($"{Name} is not suspended.");

        Status = BrokerStatus.Active;
        SuspensionReason = null;
    }
}