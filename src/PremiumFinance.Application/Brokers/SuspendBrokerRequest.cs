using System.ComponentModel.DataAnnotations;

namespace PremiumFinance.Application.Brokers;

public class SuspendBrokerRequest
{
    [Required]
    [StringLength(500)]
    public string? Reason { get; init; }

    [Required]
    public string? RowVersion { get; init; }
}