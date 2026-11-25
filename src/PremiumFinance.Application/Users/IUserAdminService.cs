namespace PremiumFinance.Application.Users;

public interface IUserAdminService
{
    Task<IReadOnlyList<UserSummary>> GetUsersAsync(CancellationToken cancellationToken);
    Task<UserSummary> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task DeactivateAsync(string userId, CancellationToken cancellationToken);
    Task ReactivateAsync(string userId, CancellationToken cancellationToken);
}
