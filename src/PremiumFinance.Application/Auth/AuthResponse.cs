namespace PremiumFinance.Application.Auth;

public record AuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresOnUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresOnUtc,
    AuthUser User);

// What the portal needs to show the user's name and decide which screens to show.
public record AuthUser(string Id, string Email, string FullName, IReadOnlyList<string> Roles);