using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PremiumFinance.Infrastructure.Persistence;

namespace PremiumFinance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PremiumFinanceDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        return services;
    }
}