using NettoRetail.Application.Services.ICountryCacheService;
using StackExchange.Redis;

namespace NettoRetail.API.Infrastructure.Extensions;

public static class CachingServiceExtensions
{
    public static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration)
    {
        var useRedis = configuration.GetValue<bool>("FeatureToggleOptions:Redis");

        if (useRedis)
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var connection = configuration.GetConnectionString("RedisConnection");
                if (string.IsNullOrEmpty(connection))
                    throw new InvalidOperationException("RedisConnection is not configured.");

                return ConnectionMultiplexer.Connect(connection);
            });

            services.AddSingleton<ICountryCacheService, RedisCountryCacheService>();
        }
        else
        {
            services.AddSingleton<ICountryCacheService, CountryCacheService>();
        }

        return services;
    }
}
