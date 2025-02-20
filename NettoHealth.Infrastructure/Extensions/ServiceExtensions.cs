using Microsoft.Extensions.DependencyInjection;
using NettoHealth.Infrastructure.Interfaces;
using NettoHealth.Infrastructure.Services;

namespace NettoHealth.Infrastructure.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructureExtensions(this IServiceCollection services)
    {
        services.AddScoped<IAzureBlobService, AzureBlobService>();
        return services;
    }
}