using System.ComponentModel.DataAnnotations;

namespace PremiumFinance.Application.Auth;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string? Email { get; init; }

    [Required]
    public string? Password { get; init; }
}