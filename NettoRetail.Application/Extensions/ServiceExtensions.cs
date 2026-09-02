using System.Reflection;
using EntityFramework.Exceptions.PostgreSQL;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.Application.Behaviors;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Services;
using NettoRetail.Application.Services.IBlobNameGenerator;
using NettoRetail.Application.Services.IPasswordService;
using NettoRetail.Application.Services.ICategoryService;
using NettoRetail.Application.Services.ISessionModelFactory;
using NettoRetail.Application.Services.PermissionCache;
using NettoRetail.DataAccess.Context;
using NettoRetail.DataAccess.Options;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Services;
using NettoRetail.Shared.Options;
using Npgsql;
using Stripe;

namespace NettoRetail.Application.Extensions;

public static class ServiceExtensions
{
    private const int ApiMaximumPoolSize = 5;
    private const int HangfireMaximumPoolSize = 2;

    public static void AddApplicationServiceExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAzureBlobService, AzureBlobService>();
        services.Configure<ConnectionStrings>(configuration.GetSection(nameof(ConnectionStrings)));

        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = BuildConnectionString(
                configuration,
                "DefaultConnection",
                ApiMaximumPoolSize,
                "netto-api");

            options.UseNpgsql(connectionString)
                .UseExceptionProcessor();
        });

        // Mediator
        services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);

        // Validation pipeline
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        //Mapping
        var typeAdapterConfig = new TypeAdapterConfig();
        typeAdapterConfig.Apply(new MappingConfiguration.MappingConfiguration());
        typeAdapterConfig.Apply(new MappingConfiguration.CategoryMappingConfiguration());
        services.AddSingleton(typeAdapterConfig);
        services.AddScoped<IMapper, ServiceMapper>();

        //Background
        // services.AddHostedService<SessionCleanupService>();
        // services.AddHostedService<TokenCleanupService>();

        //Hangfire
        services.AddHangfire(config =>
        {
            var connectionString = BuildConnectionString(
                configuration,
                "HangfireConnection",
                HangfireMaximumPoolSize,
                "netto-hangfire");

            config.UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options =>
                {
                    options.UseNpgsqlConnection(connectionString);
                });
        });
        services.AddHangfireServer(options => options.WorkerCount = 1);

        //Services
        services.AddScoped<IJwtManager, JwtManager>();
        services.AddScoped<PaymentMethodService>();
        services.AddScoped<CustomerService>();
        services.AddScoped<ISessionModelFactory, SessionModelFactory>();
        services.AddSingleton<IPermissionCacheService, PermissionCacheService>();
        services.AddScoped<IEmailBuilderService, EmailBuilderService>();
        services.AddScoped<ILinkService, LinkService>();
        services.AddScoped<IFailedAuthAttemptService, FailedAuthAttemptService>();
        services.AddScoped<IActivationEmailAttemptService, ActivationEmailAttemptAttemptService>();
        services.AddScoped<IEmailBuilderService, EmailBuilderService>();
        services.AddScoped<ISessionTerminationService, SessionTerminationService>();
        services.AddScoped<ILinkService, LinkService>();
        services.AddScoped<IBlacklistTokenService, BlacklistTokenService>();
        services.AddScoped<IPasswordService, PasswordService>();


        services.AddScoped<IBlobNameGenerator, BlobNameGenerator>();
        services.AddScoped<ChargeService>();
        services.AddScoped<ISessionTerminationService, SessionTerminationService>();
        services.AddScoped<INotificationCleanupService, NotificationCleanupService>();
        services.AddScoped<ICategoryUrlService, CategoryUrlService>();
        services.AddScoped<IStripePaymentService, StripePaymentService>();
    }

    private static string BuildConnectionString(
        IConfiguration configuration,
        string connectionName,
        int maximumPoolSize,
        string applicationName)
    {
        var connectionString = configuration
            .GetSection(nameof(ConnectionStrings))
            .GetValue<string>(connectionName)
            ?? throw new InvalidOperationException(
                $"Connection string '{connectionName}' is missing.");

        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Unknown";
        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            MinPoolSize = 0,
            MaxPoolSize = maximumPoolSize,
            ApplicationName = $"{applicationName}-{environment.ToLowerInvariant()}"
        };

        return builder.ConnectionString;
    }
}
