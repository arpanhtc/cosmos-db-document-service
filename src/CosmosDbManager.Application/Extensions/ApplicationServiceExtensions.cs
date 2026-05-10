using CosmosDbManager.Application.Interfaces;
using CosmosDbManager.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CosmosDbManager.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddValidatorsFromAssemblyContaining<IDocumentService>();

        return services;
    }
}
