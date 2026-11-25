using PremiumFinance.Domain.Brokers;
using PremiumFinance.Domain.Exceptions;

namespace PremiumFinance.UnitTests.Domain;

public class BrokerTests
{
    private static Broker Hartley() =>
        new("Hartley Insurance Brokers", "512834", "accounts@hartleybrokers.co.uk",
            CommissionDealType.ShareOfCreditCharge, 40m, 9);

    [Fact]
    public void New_broker_is_active_with_its_deal()
    {
        Broker broker = Hartley();

        Assert.Equal(BrokerStatus.Active, broker.Status);
        Assert.Equal("512834", broker.FcaReferenceNumber);
        Assert.Equal(CommissionDealType.ShareOfCreditCharge, broker.CommissionDealType);
        Assert.Equal(40m, broker.CommissionRatePercent);
        Assert.Equal(9, broker.ClawbackWindowMonths);
    }

    [Theory]
    [InlineData("12345")]      // too short
    [InlineData("12345678")]   // too long
    [InlineData("51A834")]     // not all digits
    public void Rejects_an_invalid_FCA_reference_number(string frn)
    {
        Assert.Throws<BusinessRuleException>(() =>
            new Broker("Hartley Insurance Brokers", frn, "accounts@hartleybrokers.co.uk",
                CommissionDealType.ShareOfCreditCharge, 40m, 9));
    }

    [Theory]
    [InlineData(CommissionDealType.ShareOfCreditCharge, 60.01)]
    [InlineData(CommissionDealType.PercentOfAmountFinanced, 5.01)]
    [InlineData(CommissionDealType.ShareOfCreditCharge, 0)]
    [InlineData(CommissionDealType.PercentOfAmountFinanced, 2.125)]   // 3 decimal places
    public void Rejects_a_rate_outside_the_limits_for_its_deal_type(CommissionDealType dealType, double rate)
    {
        Broker broker = Hartley();

        Assert.Throws<BusinessRuleException>(() => broker.ChangeCommissionDeal(dealType, (decimal)rate, 9));
    }

    [Fact]
    public void Suspend_needs_a_reason_and_reinstate_clears_it()
    {
        Broker broker = Hartley();

        Assert.Throws<BusinessRuleException>(() => broker.Suspend("  "));

        broker.Suspend("FCA authorisation lapsed");
        Assert.Equal(BrokerStatus.Suspended, broker.Status);
        Assert.Equal("FCA authorisation lapsed", broker.SuspensionReason);

        broker.Reinstate();
        Assert.Equal(BrokerStatus.Active, broker.Status);
        Assert.Null(broker.SuspensionReason);
    }

    [Fact]
    public void Cannot_suspend_twice()
    {
        Broker broker = Hartley();
        broker.Suspend("Suspected fraud under investigation");

        Assert.Throws<BusinessRuleException>(() => broker.Suspend("Another reason"));
    }
}