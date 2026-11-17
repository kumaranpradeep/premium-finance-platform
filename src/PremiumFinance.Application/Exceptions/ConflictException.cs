namespace PremiumFinance.Application.Exceptions;

// Thrown when the data changed underneath us, e.g. two people updating the same agreement.
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}