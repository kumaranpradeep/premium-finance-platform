namespace PremiumFinance.Application.Exceptions;

// Thrown when sign-in fails: wrong password, locked account, invalid or expired refresh token.
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}