using PremiumFinance.Domain.Brokers;

namespace PremiumFinance.Application.Brokers;

public record BrokerResponse(
    int Id,
    string Name,
    string FcaReferenceNumber,
    string ContactEmail,
    BrokerStatus Status,
    string? SuspensionReason,
    CommissionDealType CommissionDealType,
    decimal CommissionRatePercent,
    int ClawbackWindowMonths,
    string RowVersion,
    DateTime CreatedOnUtc,
    DateTime? ModifiedOnUtc)
{
    public static BrokerResponse From(Broker broker) => new(
        broker.Id,
        broker.Name,
        broker.FcaReferenceNumber,
        broker.ContactEmail,
        broker.Status,
        broker.SuspensionReason,
        broker.CommissionDealType,
        broker.CommissionRatePercent,
        broker.ClawbackWindowMonths,
        // 8 bytes from SQL Server → text the browser can store and send back, e.g. "AAAAAAAAB9E=".
        Convert.ToBase64String(broker.RowVersion),
        broker.CreatedOnUtc,
        broker.ModifiedOnUtc);
}