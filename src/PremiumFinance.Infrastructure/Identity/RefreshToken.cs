namespace PremiumFinance.Infrastructure.Identity;

// One row per refresh token issued. We store a SHA-256 hash of the token, never the token itself.
public class RefreshToken
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime CreatedOnUtc { get; set; }
    public DateTime ExpiresOnUtc { get; set; }

    // Set when the token is used (rotated), on logout, or when reuse is detected.
    public DateTime? RevokedOnUtc { get; set; }

    // The token that replaced this one, so a reused old token can be traced through the chain.
    public string? ReplacedByTokenHash { get; set; }

    public bool IsActive(DateTime nowUtc) => RevokedOnUtc is null && nowUtc < ExpiresOnUtc;
}