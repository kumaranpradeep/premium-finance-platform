namespace PremiumFinance.Domain.Common;

// Base class for every table that needs an audit trail (who created/changed a row, and when).
public abstract class AuditableEntity
{
    public DateTime CreatedOnUtc { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? ModifiedOnUtc { get; private set; }
    public string? ModifiedBy { get; private set; }

    public void MarkCreated(DateTime utcNow, string user)
    {
        CreatedOnUtc = utcNow;
        CreatedBy = user;
    }

    public void MarkModified(DateTime utcNow, string user)
    {
        ModifiedOnUtc = utcNow;
        ModifiedBy = user;
    }
}