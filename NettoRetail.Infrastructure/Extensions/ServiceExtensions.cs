using Microsoft.Extensions.DependencyInjection;
using NettoHealth.Infrastructure.Services;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Services;

namespace NettoRetail.Infrastructure.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructureExtensions(this IServiceCollection services)
    {
        services.AddScoped<IAzureBlobService, AzureBlobService>();

        services.AddHttpClient<IIpInfoService, IpInfoService>(client =>
        {
            client.BaseAddress = new Uri("https://ipinfo.io/");
        });

        return services;
    }
}