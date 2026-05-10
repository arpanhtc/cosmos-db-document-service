using CosmosDbManager.Domain.Interfaces;
using CosmosDbManager.Infrastructure.Configuration;
using CosmosDbManager.Infrastructure.CosmosDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CosmosDbManager.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CosmosDbSettings>(configuration.GetSection(CosmosDbSettings.SectionName));
        services.AddSingleton<ICosmosConnectionFactory, CosmosConnectionFactory>();
        services.AddScoped<ICosmosRepository, CosmosRepository>();

        return services;
    }
}
