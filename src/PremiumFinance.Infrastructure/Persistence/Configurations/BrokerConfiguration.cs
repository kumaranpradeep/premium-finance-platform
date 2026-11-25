using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PremiumFinance.Domain.Brokers;

namespace PremiumFinance.Infrastructure.Persistence.Configurations;

public class BrokerConfiguration : IEntityTypeConfiguration<Broker>
{
    public void Configure(EntityTypeBuilder<Broker> builder)
    {
        builder.ToTable("Brokers", table =>
        {
            table.HasCheckConstraint("CK_Brokers_CommissionRate",
                "[CommissionRatePercent] > 0 AND [CommissionRatePercent] <= 60");
            table.HasCheckConstraint("CK_Brokers_ClawbackWindow",
                "[ClawbackWindowMonths] BETWEEN 0 AND 12");
            table.HasCheckConstraint("CK_Brokers_SuspensionReason",
                "[Status] <> 'Suspended' OR [SuspensionReason] IS NOT NULL");
        });

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).HasMaxLength(200).IsRequired();
        builder.Property(b => b.FcaReferenceNumber).HasMaxLength(7).IsRequired();
        builder.Property(b => b.ContactEmail).HasMaxLength(256).IsRequired();
        builder.Property(b => b.SuspensionReason).HasMaxLength(500);
        builder.Property(b => b.CommissionRatePercent).HasPrecision(5, 2);

        // Enums stored as text ("Active", "ShareOfCreditCharge"): readable in SQL and reports,
        // and reordering the enum in code can't silently change what existing rows mean.
        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.CommissionDealType).HasConversion<string>().HasMaxLength(30);

        builder.Property(b => b.RowVersion).IsRowVersion();

        builder.Property(b => b.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(b => b.ModifiedBy).HasMaxLength(100);

        // One record per firm.
        builder.HasIndex(b => b.FcaReferenceNumber).IsUnique();
    }
}