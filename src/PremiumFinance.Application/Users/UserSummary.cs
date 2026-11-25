namespace PremiumFinance.Application.Users;

public record UserSummary(
    string Id,
    string Email,
    string FullName,
    IReadOnlyList<string> Roles,
    bool IsActive,
    bool IsLockedOut,
    DateTime CreatedOnUtc);