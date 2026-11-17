using Microsoft.Extensions.DependencyInjection;
using PremiumFinance.Application.Quotes;

namespace PremiumFinance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<QuoteService>();
        return services;
    }
}