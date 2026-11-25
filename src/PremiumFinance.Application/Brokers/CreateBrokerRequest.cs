using System.ComponentModel.DataAnnotations;
using PremiumFinance.Domain.Brokers;

namespace PremiumFinance.Application.Brokers;

public class CreateBrokerRequest
{
    [Required]
    [StringLength(200)]
    public string? Name { get; init; }

    [Required]
    [RegularExpression(@"^\d{6,7}$", ErrorMessage = "The FCA Firm Reference Number must be 6 or 7 digits.")]
    public string? FcaReferenceNumber { get; init; }

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
}