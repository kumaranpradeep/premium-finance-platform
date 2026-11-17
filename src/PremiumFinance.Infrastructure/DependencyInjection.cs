using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PremiumFinance.Application.Pricing;
using PremiumFinance.Infrastructure.Persistence;
using PremiumFinance.Infrastructure.Persistence.Repositories;

namespace PremiumFinance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PremiumFinanceDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
        services.AddScoped<IRateCardRepository, RateCardRepository>();

        return services;
    }
}