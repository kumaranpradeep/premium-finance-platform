using System.ComponentModel.DataAnnotations;

namespace PremiumFinance.Application.Auth;

public class RefreshRequest
{
    [Required]
    public string? RefreshToken { get; init; }
}