using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PremiumFinance.Application.Common;
using PremiumFinance.Application.Exceptions;
using PremiumFinance.Application.Users;
using PremiumFinance.Domain.Exceptions;
using PremiumFinance.Infrastructure.Persistence;

namespace PremiumFinance.Infrastructure.Identity;

public class UserAdminService : IUserAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly PremiumFinanceDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _clock;

    public UserAdminService(UserManager<ApplicationUser> userManager, PremiumFinanceDbContext db,
        ICurrentUser currentUser, TimeProvider clock)
    {
        _userManager = userManager;
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    private DateTime Now => _clock.GetUtcNow().UtcDateTime;

    public async Task<IReadOnlyList<UserSummary>> GetUsersAsync(CancellationToken cancellationToken)
    {
        // One query for users and their role names, instead of one extra query per user.
        var users = await _db.Users
            .AsNoTracking()
            .OrderBy(u => u.FullName)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.FullName,
                u.IsActive,
                u.LockoutEnd,
                u.CreatedOnUtc,
                Roles = _db.UserRoles
                    .Where(ur => ur.UserId == u.Id)
                    .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name!)
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        DateTimeOffset now = _clock.GetUtcNow();
        return users
            .Select(u => new UserSummary(u.Id, u.Email!, u.FullName, u.Roles, u.IsActive,
                IsLockedOut: u.LockoutEnd > now, u.CreatedOnUtc))
            .ToList();
    }

    public async Task<UserSummary> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        if (!Roles.All.Contains(request.Role))
            throw new BusinessRuleException($"'{request.Role}' is not a valid role. Valid roles: {string.Join(", ", Roles.All)}.");

        if (await _userManager.FindByEmailAsync(request.Email!) is not null)
            throw new ConflictException($"A user with the email {request.Email} already exists.");

        // EnableRetryOnFailure: our own transaction must run inside the execution strategy,
        // so that on a temporary SQL error EF can retry the WHOLE block (user + role) from the start.
        IExecutionStrategy strategy = _db.Database.CreateExecutionStrategy();

        ApplicationUser user = await strategy.ExecuteAsync(async () =>
        {
            // Start clean on every attempt: nothing left over from a failed try.
            _db.ChangeTracker.Clear();

            var newUser = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true,
                FullName = request.FullName!.Trim(),
                CreatedOnUtc = Now
            };

            // User and role in one transaction: never a user without a role.
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            IdentityResult created = await _userManager.CreateAsync(newUser, request.TemporaryPassword!);
            if (!created.Succeeded)
                throw new BusinessRuleException(string.Join(" ", created.Errors.Select(e => e.Description)));

            IdentityResult roleAdded = await _userManager.AddToRoleAsync(newUser, request.Role!);
            if (!roleAdded.Succeeded)
                throw new BusinessRuleException(string.Join(" ", roleAdded.Errors.Select(e => e.Description)));

            await transaction.CommitAsync(cancellationToken);
            return newUser;
        });

        return new UserSummary(user.Id, user.Email!, user.FullName, [request.Role!], user.IsActive,
            IsLockedOut: false, user.CreatedOnUtc);
    }

    public async Task DeactivateAsync(string userId, CancellationToken cancellationToken)
    {
        if (userId == _currentUser.UserId)
            throw new BusinessRuleException("You can't deactivate your own account.");

        ApplicationUser user = await FindUserAsync(userId);
        user.IsActive = false;
        await _userManager.UpdateAsync(user);

        // Sign them out everywhere: no more refreshes. Their current access token dies within 15 minutes.
        DateTime now = Now;
        await _db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedOnUtc == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.RevokedOnUtc, now), cancellationToken);
    }

    public async Task ReactivateAsync(string userId, CancellationToken cancellationToken)
    {
        ApplicationUser user = await FindUserAsync(userId);
        user.IsActive = true;
        await _userManager.UpdateAsync(user);
    }

    private async Task<ApplicationUser> FindUserAsync(string userId) =>
        await _userManager.FindByIdAsync(userId)
        ?? throw new NotFoundException($"User {userId} was not found.");
}