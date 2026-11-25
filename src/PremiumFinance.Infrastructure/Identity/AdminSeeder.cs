using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PremiumFinance.Application.Common;

namespace PremiumFinance.Infrastructure.Identity;

// Creates the very first Admin from configuration, so a brand-new environment has someone who can create users.
public class AdminSeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly TimeProvider _clock;
    private readonly ILogger<AdminSeeder> _logger;

    public AdminSeeder(UserManager<ApplicationUser> userManager, IConfiguration configuration,
        TimeProvider clock, ILogger<AdminSeeder> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _clock = clock;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        string? email = _configuration["Seed:AdminEmail"];
        string? password = _configuration["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogInformation("No first admin configured; skipping admin seeding.");
            return;
        }

        // Only ever runs once: as soon as any Admin exists, this does nothing.
        IList<ApplicationUser> admins = await _userManager.GetUsersInRoleAsync(Roles.Admin);
        if (admins.Count > 0)
            return;

        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = _configuration["Seed:AdminName"] ?? "System Administrator",
            CreatedOnUtc = _clock.GetUtcNow().UtcDateTime
        };

        IdentityResult created = await _userManager.CreateAsync(admin, password);
        if (!created.Succeeded)
            throw new InvalidOperationException(
                "Could not create the first admin: " + string.Join(" ", created.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(admin, Roles.Admin);
        _logger.LogInformation("Created the first admin user {Email}.", email);
    }
}