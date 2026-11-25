using System.ComponentModel.DataAnnotations;
using PremiumFinance.Domain.Brokers;

namespace PremiumFinance.Application.Brokers;

// The FCA number is not here: it identifies the firm and never changes.
public class UpdateBrokerRequest
{
    [Required]
    [StringLength(200)]
    public string? Name { get; init; }

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string? ContactEmail { get; init; }

    [Required]
    [EnumDataType(typeof(CommissionDealType))]
    public CommissionDealType? CommissionDealType { get; init; }

    [Required]
    [Range(typeof(decimal), "0.01", "60", ErrorMessage = "The commission rate must be between 0.01% and 60%.")]
    public decimal? CommissionRatePercent { get; init; }

    [Required]
    [Range(0, 12, ErrorMessage = "The clawback window must be between 0 and 12 months.")]
    public int? ClawbackWindowMonths { get; init; }

    // The rowVersion the user saw when they opened the broker.
    [Required]
    public string? RowVersion { get; init; }
}