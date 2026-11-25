using Microsoft.EntityFrameworkCore;
using PremiumFinance.Application.Common;
using PremiumFinance.Domain.Common;
using PremiumFinance.Domain.Pricing;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using PremiumFinance.Infrastructure.Identity;

namespace PremiumFinance.Infrastructure.Persistence;

public class PremiumFinanceDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _clock;

    public PremiumFinanceDbContext(DbContextOptions<PremiumFinanceDbContext> options, ICurrentUser currentUser, TimeProvider clock)
        : base(options)
    {
        _currentUser = currentUser;
        _clock = clock;
    }
    public DbSet<RateCardEntry> RateCardEntries => Set<RateCardEntry>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Identity's own tables and keys (AspNetUsers, AspNetRoles...). Must come first.
        base.OnModelCreating(modelBuilder);

        // Each entity gets its own IEntityTypeConfiguration class in this assembly; picked up automatically.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PremiumFinanceDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAuditColumns();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        StampAuditColumns();
        return base.SaveChanges();
    }

    private void StampAuditColumns()
    {
        DateTime now = _clock.GetUtcNow().UtcDateTime;
        string user = _currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.MarkCreated(now, user);
            else if (entry.State == EntityState.Modified)
                entry.Entity.MarkModified(now, user);
        }
    }
}