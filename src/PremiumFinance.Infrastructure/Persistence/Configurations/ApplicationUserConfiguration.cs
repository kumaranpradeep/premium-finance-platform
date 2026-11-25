using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PremiumFinance.Domain.Brokers;
using PremiumFinance.Infrastructure.Identity;

namespace PremiumFinance.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // A broker user points at their firm. Restrict: a firm with users can't be deleted
        // (firms are suspended, never deleted).
        builder.HasOne<Broker>()
            .WithMany()
            .HasForeignKey(u => u.BrokerId)
            .OnDelete(DeleteBehavior.Restrict); 
    }
}