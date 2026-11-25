using PremiumFinance.Domain.Brokers;

namespace PremiumFinance.Application.Brokers;

public interface IBrokerRepository
{
    Task<IReadOnlyList<Broker>> GetAllAsync(CancellationToken cancellationToken);
    Task<Broker?> GetByIdAsync(int id, CancellationToken cancellationToken);

    // Loads the broker for changing, and tells EF which version the user saw,
    // so saving fails if someone else changed it in the meantime.
    Task<Broker?> GetForUpdateAsync(int id, byte[] expectedRowVersion, CancellationToken cancellationToken);

    Task<bool> FcaReferenceNumberExistsAsync(string fcaReferenceNumber, CancellationToken cancellationToken);
    void Add(Broker broker);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}