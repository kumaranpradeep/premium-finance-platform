using System.ComponentModel.DataAnnotations;

namespace PremiumFinance.Application.Brokers;

// For actions that change a broker but need no other input, e.g. reinstate.
public class RowVersionRequest
{
    [Required]
    public string? RowVersion { get; init; }
}