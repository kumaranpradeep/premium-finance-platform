using System.ComponentModel.DataAnnotations;

namespace PremiumFinance.Application.Users;

public class CreateUserRequest
{
    [Required]
    [StringLength(200)]
    public string? FullName { get; init; }

    [Required]
    [EmailAddress]
    public string? Email { get; init; }

    [Required]
    public string? Role { get; init; }

    // Given to the user by the Admin; Identity's password rules (12+ characters, etc.) apply.
    [Required]
    public string? TemporaryPassword { get; init; }
}