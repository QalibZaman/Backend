using Azure.Storage.Blobs;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NettoHealth.DataAccess.Context;
using NettoHealth.DataAccess.Options;
using NettoHealth.DataAccess.Repositories;
using NettoHealth.Shared.Interfaces.Repositories;

namespace NettoHealth.DataAccess.Extensions;

public static class ServiceExtensions
{
    public static void AddDataAccessServiceExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        // Options
        services.Configure<ConnectionStrings>(configuration.GetSection(nameof(ConnectionStrings)));

        // External clients
        services.AddScoped<BlobServiceClient>(provider =>
        {
            var connectionStrings = provider.GetRequiredService<IOptions<ConnectionStrings>>().Value;
            return new BlobServiceClient(connectionStrings.AzureConnection);
        });

        // Repositories
        services.AddScoped<IHealthcareProviderRepository, HealthcareProviderRepository>();

        // Database related
        services.AddDbContext<AppDbContext>();
    }
}