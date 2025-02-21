using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NettoRetail.DataAccess.Context;
using NettoRetail.DataAccess.Options;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Extensions;

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
        services.AddScoped<IStaffRepository, StaffRepository>();

        // Database related
        services.AddDbContext<AppDbContext>();
    }
}