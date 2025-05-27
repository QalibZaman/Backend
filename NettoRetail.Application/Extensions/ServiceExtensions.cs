using System.Reflection;
using Hangfire;
using Hangfire.PostgreSql;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Services;
using NettoRetail.Application.Services.ICountryCacheService;
using NettoRetail.Application.Services.ICountryValidationService;
using NettoRetail.Application.Services.INewsletterService;
using NettoRetail.Application.Services.ISessionCleanupService;
using NettoRetail.Application.Services.ISessionModelFactory;
using NettoRetail.Application.Services.PermissionCache;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Options;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;

namespace NettoRetail.Application.Extensions;

public static class ServiceExtensions
{
    public static void AddApplicationServiceExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        // Mediator
        services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        //Mapping
        services.AddMapster();
        TypeAdapterConfig.GlobalSettings.Apply(new MappingConfiguration.MappingConfiguration());

        //Background
        services.AddHostedService<SessionCleanupService>();
        services.AddHostedService<TokenCleanupService>();

        //Hangfire
        services.AddHangfire(config =>
        {
            var connectionString = configuration.GetSection(nameof(ConnectionStrings)).GetValue<string>("HangfireConnection");

            config.UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options =>
                {
                    options.UseNpgsqlConnection(connectionString);
                });
        });
        services.AddHangfireServer();

        //Services
        services.AddScoped<IFailedAuthAttemptService, FailedAuthAttemptService>();
        services.AddScoped<ISessionModelFactory, SessionModelFactory>();
        services.AddSingleton<IPermissionCacheService, PermissionCacheService>();
        services.AddScoped<ICountryDetailsValidationService, CountryDetailsValidationService>();
        services.AddScoped<ICategoryLevelManagementService, CategoryLevelManagementService>();
        services.AddScoped<IJwtManager, JwtManager>();
        services.AddScoped<IBlacklistTokenService, BlacklistTokenService>();
        services.AddScoped<ITotpService, TotpService>();
        services.AddScoped<ICsvFileService, CsvFileService>();
        services.AddScoped<IEmailBuilderService, EmailBuilderService>();
        services.AddScoped<ISessionTerminationService, SessionTerminationService>();
        services.AddScoped<INewsletterService, NewsletterService>();

        var featureToggleOptions = configuration.GetSection(nameof(FeatureToggleOptions)).Get<FeatureToggleOptions>();

        if (featureToggleOptions != null && featureToggleOptions.Redis)
        {
            services.AddSingleton<ICountryCacheService, RedisCountryCacheService>();
        }
        else
        {
            services.AddSingleton<ICountryCacheService, CountryCacheService>();
        }
    }
}