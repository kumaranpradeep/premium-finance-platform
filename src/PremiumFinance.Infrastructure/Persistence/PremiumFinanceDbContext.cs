using Microsoft.EntityFrameworkCore;
using PremiumFinance.Application.Common;
using PremiumFinance.Domain.Common;

namespace PremiumFinance.Infrastructure.Persistence;

public class PremiumFinanceDbContext : DbContext
{
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _clock;

    public PremiumFinanceDbContext(DbContextOptions<PremiumFinanceDbContext> options, ICurrentUser currentUser, TimeProvider clock)
        : base(options)
    {
        _currentUser = currentUser;
        _clock = clock;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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