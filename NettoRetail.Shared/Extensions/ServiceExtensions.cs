using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Options;

namespace NettoRetail.Shared.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddSharedServiceExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(nameof(JwtOptions)));
        services.Configure<AdminRoleOptions>(configuration.GetSection(nameof(AdminRoleOptions)));
        services.Configure<UserBlockOptions>(configuration.GetSection(nameof(UserBlockOptions)));
        services.Configure<EmailSendBlockOptions>(configuration.GetSection(nameof(EmailSendBlockOptions)));
        services.Configure<LockedAccountActivationOptions>(configuration.GetSection(nameof(LockedAccountActivationOptions)));
        return services;
    }
}