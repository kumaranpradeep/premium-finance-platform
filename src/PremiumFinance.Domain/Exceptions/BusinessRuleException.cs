namespace PremiumFinance.Domain.Exceptions;

// Thrown when an action would break a business rule, e.g. recording a payment on a cancelled agreement.
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}