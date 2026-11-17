namespace PremiumFinance.Application.Common;

// Who is performing the current action. Implemented in the Api (from the JWT); "system" for background jobs.
public interface ICurrentUser
{
    string UserId { get; }
}