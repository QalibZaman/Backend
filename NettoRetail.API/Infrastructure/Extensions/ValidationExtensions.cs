using FluentValidation.AspNetCore;

namespace NettoRetail.API.Infrastructure.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection ConfigureValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();

        return services;
    }
}