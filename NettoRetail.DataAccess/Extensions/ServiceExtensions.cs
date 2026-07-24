using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NettoRetail.DataAccess.Context;
using NettoRetail.DataAccess.Options;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Seeders.Interfaces;
using NettoRetail.DataAccess.UOW;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.DataAccess.Extensions;

public static class ServiceExtensions
{
    public static void AddDataAccessServiceExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        // Options
        services.Configure<ConnectionStrings>(configuration.GetSection(nameof(ConnectionStrings)));
        services.Configure<CosmosOptions>(configuration.GetSection(nameof(CosmosOptions)));
        services.Configure<AzureBlobStorageOptions>(configuration.GetSection(nameof(AzureBlobStorageOptions)));

        // External clients
        services.AddScoped<BlobServiceClient>(provider =>
        {
            var connectionStrings = provider.GetRequiredService<IOptions<ConnectionStrings>>().Value;
            return new BlobServiceClient(connectionStrings.AzureConnection);
        });

        services.AddSingleton<CosmosClient>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<CosmosOptions>>().Value;

            return new CosmosClient(
                options.AccountEndpoint,
                options.AccountKey,
                new CosmosClientOptions
                {
                    ConnectionMode = ConnectionMode.Gateway,
                    HttpClientFactory = () => new HttpClient(new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (req, cert, chain, errors) => true
                    }),
                    SerializerOptions = new CosmosSerializationOptions
                    {
                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
                        IgnoreNullValues = true
                    }
                });
        });

        // Repositories
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IPermissionRoleRepository, PermissionRoleRepsitory>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IFailedAuthAttemptRepository, FailedAuthAttemptRepository>();
        services.AddScoped<IActivationEmailAttemptRepository, ActivationEmailAttemptRepository>();

        //UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Seeders
       
        // Database related
        services.AddDbContext<AppDbContext>();
    }
}
