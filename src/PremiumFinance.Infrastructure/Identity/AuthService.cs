using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PremiumFinance.Application.Auth;
using PremiumFinance.Application.Exceptions;
using PremiumFinance.Infrastructure.Persistence;

namespace PremiumFinance.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private const string InvalidCredentials = "Invalid email or password.";
    private const string InvalidRefreshToken = "Your session is no longer valid. Please sign in again.";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly PremiumFinanceDbContext _db;
    private readonly JwtTokenGenerator _tokens;
    private readonly IConfiguration _configuration;
    private readonly TimeProvider _clock;

    public AuthService(UserManager<ApplicationUser> userManager, PremiumFinanceDbContext db,
        JwtTokenGenerator tokens, IConfiguration configuration, TimeProvider clock)
    {
        _userManager = userManager;
        _db = db;
        _tokens = tokens;
        _configuration = configuration;
        _clock = clock;
    }

    private DateTime Now => _clock.GetUtcNow().UtcDateTime;

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        ApplicationUser? user = await _userManager.FindByEmailAsync(request.Email!);

        // Same message for "no such user" and "wrong password", so nobody can find out which emails exist.
        if (user is null || !user.IsActive)
            throw new UnauthorizedException(InvalidCredentials);

        if (await _userManager.IsLockedOutAsync(user))
            throw new UnauthorizedException("This account is locked after too many failed sign-ins. Try again in 15 minutes.");

        if (!await _userManager.CheckPasswordAsync(user, request.Password!))
        {
            // Counts the failure; on the 5th, Identity sets LockoutEnd to now + 15 minutes.
            await _userManager.AccessFailedAsync(user);
            throw new UnauthorizedException(InvalidCredentials);
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        return await IssueTokensAsync(user, replacing: null, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        RefreshToken? stored = await FindAsync(refreshToken, cancellationToken);
        if (stored is null)
            throw new UnauthorizedException(InvalidRefreshToken);

        if (stored.RevokedOnUtc is not null)
        {
            // An already-used token came back: either it was stolen, or the real user's copy was.
            // We can't tell which, so we sign the user out everywhere.
            await RevokeAllForUserAsync(stored.UserId, cancellationToken);
            throw new UnauthorizedException(InvalidRefreshToken);
        }

        if (Now >= stored.ExpiresOnUtc)
            throw new UnauthorizedException(InvalidRefreshToken);

        ApplicationUser? user = await _userManager.FindByIdAsync(stored.UserId);
        if (user is null || !user.IsActive || await _userManager.IsLockedOutAsync(user))
            throw new UnauthorizedException(InvalidRefreshToken);

        return await IssueTokensAsync(user, replacing: stored, cancellationToken);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        RefreshToken? stored = await FindAsync(refreshToken, cancellationToken);

        // Logging out twice, or with an unknown token, is not an error: the result is the same.
        if (stored is null || stored.RevokedOnUtc is not null)
            return;

        stored.RevokedOnUtc = Now;
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthResponse> IssueTokensAsync(ApplicationUser user, RefreshToken? replacing,
        CancellationToken cancellationToken)
    {
        DateTime now = Now;
        IList<string> roles = await _userManager.GetRolesAsync(user);
        AccessToken accessToken = _tokens.CreateAccessToken(user, roles);

        // 64 random bytes from the OS's secure random generator: impossible to guess.
        string refreshToken = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(64));

        var newToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = Hash(refreshToken),
            CreatedOnUtc = now,
            ExpiresOnUtc = now.AddDays(int.Parse(_configuration["Jwt:RefreshTokenDays"]!))
        };
        _db.RefreshTokens.Add(newToken);

        // Rotation: the token just used is cancelled and points to its replacement.
        if (replacing is not null)
        {
            replacing.RevokedOnUtc = now;
            replacing.ReplacedByTokenHash = newToken.TokenHash;
        }

        // New token and revoked old token are saved together, in one transaction.
        await _db.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            accessToken.Value,
            accessToken.ExpiresOnUtc,
            refreshToken,
            newToken.ExpiresOnUtc,
            new AuthUser(user.Id, user.Email!, user.FullName, roles.ToList()));
    }

    private Task<RefreshToken?> FindAsync(string refreshToken, CancellationToken cancellationToken)
    {
        string hash = Hash(refreshToken);
        return _db.RefreshTokens.SingleOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);
    }

    private Task RevokeAllForUserAsync(string userId, CancellationToken cancellationToken)
    {
        DateTime now = Now;
        // One UPDATE statement in the database, without loading the rows.
        return _db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedOnUtc == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.RevokedOnUtc, now), cancellationToken);
    }

    // SHA-256 is fine here (unlike for passwords): the token is 64 random bytes, so there's nothing to guess.
    private static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}