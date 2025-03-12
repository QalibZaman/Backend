using System.Reflection;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Services;
using NettoRetail.Application.Services.ICountryCacheService;
using NettoRetail.Application.Services.ICountryValidationService;
using NettoRetail.Application.Services.ISessionCleanupService;
using NettoRetail.Application.Services.ISessionModelFactory;

namespace NettoRetail.Application.Extensions;

public static class ServiceExtensions
{
    public static void AddApplicationServiceExtensions(this IServiceCollection services)
    {
        // Mediator
        services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        //Mapping
        services.AddMapster();
        TypeAdapterConfig.GlobalSettings.Apply(new MappingConfiguration.MappingConfiguration());

        //Background
        services.AddHostedService<SessionCleanupService>();
        services.AddHostedService<TokenCleanupService>();

        //Services
        services.AddScoped<IFailedAuthAttemptService, FailedAuthAttemptService>();
        services.AddScoped<ISessionModelFactory, SessionModelFactory>();
        services.AddScoped<ICountryCacheService, CountryCacheService>();
        services.AddScoped<ICountryDetailsValidationService, CountryDetailsValidationService>();
        services.AddScoped<ICategoryLevelManagementService, CategoryLevelManagementService>();
        services.AddScoped<IJwtManager, JwtManager>();
        services.AddScoped<IBlacklistTokenService, BlacklistTokenService>();
    }
}