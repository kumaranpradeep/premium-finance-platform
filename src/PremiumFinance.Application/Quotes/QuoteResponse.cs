namespace PremiumFinance.Application.Quotes;

public record QuoteInstalmentResponse(int Number, DateOnly DueDate, decimal Amount);

public record QuoteResponse(
    decimal Premium,
    int NumberOfInstalments,
    decimal AnnualFlatRatePercent,
    decimal CreditCharge,
    decimal TotalPayable,
    decimal Apr,
    DateOnly PolicyStartDate,
    IReadOnlyList<QuoteInstalmentResponse> Instalments);