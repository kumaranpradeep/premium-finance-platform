using PremiumFinance.Application.Exceptions;
using PremiumFinance.Domain.Brokers;
using PremiumFinance.Domain.Exceptions;

namespace PremiumFinance.Application.Brokers;

public class BrokerService
{
    private readonly IBrokerRepository _brokers;

    public BrokerService(IBrokerRepository brokers) => _brokers = brokers;

    public async Task<IReadOnlyList<BrokerResponse>> GetBrokersAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<Broker> brokers = await _brokers.GetAllAsync(cancellationToken);
        return brokers.Select(BrokerResponse.From).ToList();
    }

    public async Task<BrokerResponse> GetBrokerAsync(int id, CancellationToken cancellationToken)
    {
        Broker broker = await _brokers.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Broker {id} was not found.");
        return BrokerResponse.From(broker);
    }

    public async Task<BrokerResponse> CreateBrokerAsync(CreateBrokerRequest request, CancellationToken cancellationToken)
    {
        if (await _brokers.FcaReferenceNumberExistsAsync(request.FcaReferenceNumber!, cancellationToken))
            throw new ConflictException($"A broker with FCA Firm Reference Number {request.FcaReferenceNumber} already exists.");

        var broker = new Broker(
            request.Name!,
            request.FcaReferenceNumber!,
            request.ContactEmail!,
            request.CommissionDealType!.Value,
            request.CommissionRatePercent!.Value,
            request.ClawbackWindowMonths!.Value);

        _brokers.Add(broker);
        await _brokers.SaveChangesAsync(cancellationToken);   // fills Id and RowVersion from the database

        return BrokerResponse.From(broker);
    }

    public async Task<BrokerResponse> UpdateBrokerAsync(int id, UpdateBrokerRequest request, CancellationToken cancellationToken)
    {
        Broker broker = await GetForUpdateAsync(id, request.RowVersion!, cancellationToken);

        broker.UpdateDetails(request.Name!, request.ContactEmail!);
        broker.ChangeCommissionDeal(
            request.CommissionDealType!.Value,
            request.CommissionRatePercent!.Value,
            request.ClawbackWindowMonths!.Value);

        await _brokers.SaveChangesAsync(cancellationToken);
        return BrokerResponse.From(broker);
    }

    public async Task<BrokerResponse> SuspendBrokerAsync(int id, SuspendBrokerRequest request, CancellationToken cancellationToken)
    {
        Broker broker = await GetForUpdateAsync(id, request.RowVersion!, cancellationToken);
        broker.Suspend(request.Reason!);
        await _brokers.SaveChangesAsync(cancellationToken);
        return BrokerResponse.From(broker);
    }

    public async Task<BrokerResponse> ReinstateBrokerAsync(int id, RowVersionRequest request, CancellationToken cancellationToken)
    {
        Broker broker = await GetForUpdateAsync(id, request.RowVersion!, cancellationToken);
        broker.Reinstate();
        await _brokers.SaveChangesAsync(cancellationToken);
        return BrokerResponse.From(broker);
    }

    private async Task<Broker> GetForUpdateAsync(int id, string rowVersion, CancellationToken cancellationToken)
    {
        byte[] expected;
        try
        {
            expected = Convert.FromBase64String(rowVersion);
        }
        catch (FormatException)
        {
            throw new BusinessRuleException("The rowVersion is not valid. Reload the broker and try again.");
        }

        return await _brokers.GetForUpdateAsync(id, expected, cancellationToken)
            ?? throw new NotFoundException($"Broker {id} was not found.");
    }
}