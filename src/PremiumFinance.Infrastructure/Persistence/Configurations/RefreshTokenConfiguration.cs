using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PremiumFinance.Infrastructure.Identity;

namespace PremiumFinance.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(t => t.Id);

        // Same length as Identity's user ID column.
        builder.Property(t => t.UserId).HasMaxLength(450).IsRequired();

        // SHA-256 written as hex is always 64 characters.
        builder.Property(t => t.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(t => t.ReplacedByTokenHash).HasMaxLength(64);

        // Every refresh looks a token up by its hash: unique index = fast lookup, no duplicates.
        builder.HasIndex(t => t.TokenHash).IsUnique();

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}