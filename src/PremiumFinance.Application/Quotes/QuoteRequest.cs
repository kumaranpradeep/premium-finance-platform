using System.ComponentModel.DataAnnotations;

namespace PremiumFinance.Application.Quotes;

public class QuoteRequest
{
    [Required]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "The premium must be more than £0.")]
    public decimal? Premium { get; init; }

    [Required]
    [Range(1, 12, ErrorMessage = "Choose between 1 and 12 monthly instalments.")]
    public int? NumberOfInstalments { get; init; }

    [Required]
    public DateOnly? PolicyStartDate { get; init; }
}