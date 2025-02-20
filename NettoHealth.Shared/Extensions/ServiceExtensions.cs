using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NettoHealth.Shared.Interfaces.Utilities;
using NettoHealth.Shared.Options;
using NettoHealth.Shared.Utilities;

namespace NettoHealth.Shared.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddSharedServiceExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(nameof(JwtOptions)));
        services.AddScoped<IJwtManager, JwtManager>();
        return services;
    }
}