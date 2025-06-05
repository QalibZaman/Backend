using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NettoRetail.DataAccess.Context;
using NettoRetail.DataAccess.Options;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Seeders;
using NettoRetail.DataAccess.Seeders.Interfaces;
using NettoRetail.DataAccess.UOW;
using NettoRetail.Domain.Entities;
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
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IFailedAuthAttemptRepository, FailedAuthAttemptRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IContentRepository, ContentRepository>();
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<IShopRepository, ShopRepository>();
        services.AddScoped<IBlacklistedTokenRepository, BlacklistedTokenRepository>();
        services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IProductDetailsRepository, ProductDetailsRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<IPermissionRoleRepository, PermissionRoleRepsitory>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<ISubcriptionPlanRepository, SubcriptionPlanRepository>();
        services.AddScoped<IPasswordHistoryRepository, PasswordHistoryRepository>();
        services.AddScoped<IPaymentHistoryRepository, PaymentHistoryRepository>();

        //UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Seeders
        services.AddScoped<IDatabaseSeeder, SubscriptionPlanSeeder>();

        // Database related
        services.AddDbContext<AppDbContext>();
    }
}