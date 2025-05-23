using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.Shared.Options;

namespace NettoRetail.Shared.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddSharedServiceExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(nameof(JwtOptions)));
        services.Configure<LevelOptions>(configuration.GetSection(nameof(LevelOptions)));
        services.Configure<UserBlockOptions>(configuration.GetSection(nameof(UserBlockOptions)));
        services.Configure<TokenBlacklistOptions>(configuration.GetSection(nameof(TokenBlacklistOptions)));
        services.Configure<AdminRoleOptions>(configuration.GetSection(nameof(AdminRoleOptions)));
        services.Configure<BatchOptions>(configuration.GetSection(nameof(BatchOptions)));
        services.Configure<SearchOptions>(configuration.GetSection(nameof(SearchOptions)));
        services.Configure<TotpOptions>(configuration.GetSection(nameof(TotpOptions)));
        services.Configure<TotpEncryptionOptions>(configuration.GetSection(nameof(TotpEncryptionOptions)));
        services.Configure<AdminRoleOptions>(configuration.GetSection(nameof(AdminRoleOptions)));
        services.Configure<SessionTrackingOptions>(configuration.GetSection(nameof(SessionTrackingOptions)));
        services.Configure<SubscriptionOptions>(configuration.GetSection(nameof(SubscriptionOptions)));
        return services;
    }
}