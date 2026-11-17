using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PremiumFinance.Domain.Pricing;

namespace PremiumFinance.Infrastructure.Persistence.Configurations;

public class RateCardEntryConfiguration : IEntityTypeConfiguration<RateCardEntry>
{
    public void Configure(EntityTypeBuilder<RateCardEntry> builder)
    {
        builder.ToTable("RateCardEntries", table =>
        {
            table.HasCheckConstraint("CK_RateCardEntries_Instalments", "[NumberOfInstalments] BETWEEN 1 AND 12");
            table.HasCheckConstraint("CK_RateCardEntries_Rate", "[AnnualFlatRatePercent] > 0 AND [AnnualFlatRatePercent] <= 50");
            table.HasCheckConstraint("CK_RateCardEntries_Dates", "[EffectiveTo] IS NULL OR [EffectiveTo] >= [EffectiveFrom]");
        });

        builder.HasKey(r => r.Id);
        builder.Property(r => r.SchemeCode).HasMaxLength(20).IsRequired();
        builder.Property(r => r.AnnualFlatRatePercent).HasPrecision(5, 2);
        builder.Property(r => r.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(r => r.ModifiedBy).HasMaxLength(100);

        // One rate per scheme + term starting on a given date.
        builder.HasIndex(r => new { r.SchemeCode, r.NumberOfInstalments, r.EffectiveFrom }).IsUnique();

        // Initial rates, set by finance. Reference data like this is delivered by migrations.
        var seededOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        builder.HasData(
            new { Id = 1, SchemeCode = "STANDARD", NumberOfInstalments = 10, AnnualFlatRatePercent = 9.50m,
                  EffectiveFrom = new DateOnly(2026, 1, 1), EffectiveTo = (DateOnly?)null, CreatedOnUtc = seededOn, CreatedBy = "migration" },
            new { Id = 2, SchemeCode = "STANDARD", NumberOfInstalments = 11, AnnualFlatRatePercent = 9.75m,
                  EffectiveFrom = new DateOnly(2026, 1, 1), EffectiveTo = (DateOnly?)null, CreatedOnUtc = seededOn, CreatedBy = "migration" },
            new { Id = 3, SchemeCode = "STANDARD", NumberOfInstalments = 12, AnnualFlatRatePercent = 9.90m,
                  EffectiveFrom = new DateOnly(2026, 1, 1), EffectiveTo = (DateOnly?)new DateOnly(2026, 12, 31), CreatedOnUtc = seededOn, CreatedBy = "migration" },
            new { Id = 4, SchemeCode = "STANDARD", NumberOfInstalments = 12, AnnualFlatRatePercent = 10.50m,
                  EffectiveFrom = new DateOnly(2027, 1, 1), EffectiveTo = (DateOnly?)null, CreatedOnUtc = seededOn, CreatedBy = "migration" });
    }
}