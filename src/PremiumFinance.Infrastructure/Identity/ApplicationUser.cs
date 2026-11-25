using Microsoft.AspNetCore.Identity;

namespace PremiumFinance.Infrastructure.Identity;

// Identity's user (Id, Email, PasswordHash, lockout fields...) plus our own columns.
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    // Leavers are deactivated, never deleted: their ID stays in the audit columns (CreatedBy/ModifiedBy).
    public bool IsActive { get; set; } = true;

    public DateTime CreatedOnUtc { get; set; }
        // Set for Broker users only: the firm they work for. Their access is limited to that firm's business.
    public int? BrokerId { get; set; }
}