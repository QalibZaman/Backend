using NettoRetail.API.Infrastructure.Helpers.Authentication;
using NettoRetail.Application.Authentication;

namespace NettoRetail.API.Infrastructure.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddAPIServiceExtensions(this IServiceCollection services)
    {
        services.AddScoped<IUserContext, UserContext>();

        return services;
    }
}