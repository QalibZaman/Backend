using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Shared.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddSharedServiceExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(nameof(JwtOptions)));
        services.AddScoped<IJwtManager, JwtManager>();
        return services;
    }
}