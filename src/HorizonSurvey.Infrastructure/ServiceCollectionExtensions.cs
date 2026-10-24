using HorizonSurvey.Domain.Contracts;
using HorizonSurvey.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HorizonSurvey.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInventoryInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("HorizonSurvey");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Connection string 'HorizonSurvey' is required. Configure ConnectionStrings__HorizonSurvey.");

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseSqlServer(connectionString, sqlServer =>
                sqlServer.MigrationsAssembly(typeof(InventoryDbContext).Assembly.FullName)));
        services.AddScoped<IInventoryRepository, EfInventoryRepository>();
        return services;
    }
}
