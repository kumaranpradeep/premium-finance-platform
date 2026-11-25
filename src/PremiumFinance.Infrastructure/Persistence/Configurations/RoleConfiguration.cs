using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PremiumFinance.Application.Common;

namespace PremiumFinance.Infrastructure.Persistence.Configurations;

// The roles are fixed by the code, so they're seeded by a migration
// and exist in every environment (local, Azure) without anyone creating them by hand.
public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            Role("0b6f5c2e-1d4a-4e8b-9a37-2f1c6d8e4a01", Roles.Admin),
            Role("1c7a6d3f-2e5b-4f9c-8b48-3a2d7e9f5b02", Roles.Broker),
            Role("2d8b7e4a-3f6c-4a1d-9c59-4b3e8f1a6c03", Roles.Agent),
            Role("3e9c8f5b-4a7d-4b2e-8d6a-5c4f9a2b7d04", Roles.TeamLeader),
            Role("4f1d9a6c-5b8e-4c3f-9e7b-6d5a1b3c8e05", Roles.Underwriter),
            Role("5a2e1b7d-6c9f-4d4a-8f8c-7e6b2c4d9f06", Roles.Operations),
            Role("6b3f2c8e-7d1a-4e5b-9a9d-8f7c3d5e1a07", Roles.Compliance));
    }

    // Fixed Id and ConcurrencyStamp: if they were generated (Guid.NewGuid()), every new migration
    // would think the roles had changed and try to update them.
    private static IdentityRole Role(string id, string name) => new()
    {
        Id = id,
        Name = name,
        NormalizedName = name.ToUpperInvariant(),
        ConcurrencyStamp = id
    };
}