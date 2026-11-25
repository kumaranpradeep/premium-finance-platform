using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PremiumFinance.Application.Brokers;
using PremiumFinance.Application.Exceptions;
using PremiumFinance.Domain.Brokers;

namespace PremiumFinance.Infrastructure.Persistence.Repositories;

public class BrokerRepository : IBrokerRepository
{
    private readonly PremiumFinanceDbContext _db;

    public BrokerRepository(PremiumFinanceDbContext db) => _db = db;

    public async Task<IReadOnlyList<Broker>> GetAllAsync(CancellationToken cancellationToken) =>
        await _db.Brokers.AsNoTracking().OrderBy(b => b.Name).ToListAsync(cancellationToken);

    public Task<Broker?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        _db.Brokers.AsNoTracking().SingleOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<Broker?> GetForUpdateAsync(int id, byte[] expectedRowVersion, CancellationToken cancellationToken)
    {
        Broker? broker = await _db.Brokers.SingleOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (broker is not null)
        {
            // EF puts "WHERE RowVersion = <original value>" on the UPDATE.
            // The original must be what the USER saw, not what we just loaded.
            _db.Entry(broker).Property(b => b.RowVersion).OriginalValue = expectedRowVersion;
        }

        return broker;
    }

    public Task<bool> FcaReferenceNumberExistsAsync(string fcaReferenceNumber, CancellationToken cancellationToken) =>
        _db.Brokers.AnyAsync(b => b.FcaReferenceNumber == fcaReferenceNumber, cancellationToken);

    public void Add(Broker broker) => _db.Brokers.Add(broker);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "This broker was changed by someone else after you opened it. Reload it to see the latest details, then make your change again.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // Unique index hit: two people set up the same firm at the same moment.
            throw new ConflictException("A broker with this FCA Firm Reference Number already exists.");
        }
    }
}