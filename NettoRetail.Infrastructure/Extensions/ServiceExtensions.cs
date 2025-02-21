using Microsoft.Extensions.DependencyInjection;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Services;

namespace NettoRetail.Infrastructure.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructureExtensions(this IServiceCollection services)
    {
        services.AddScoped<IAzureBlobService, AzureBlobService>();
        return services;
    }
}