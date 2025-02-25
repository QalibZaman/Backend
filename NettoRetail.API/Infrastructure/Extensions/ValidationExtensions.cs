using FluentValidation;
using FluentValidation.AspNetCore;
using NettoRetail.API.Infrastructure.Validators;

namespace NettoRetail.API.Infrastructure.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection ConfigureValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<AdminSignUpValidator>();
        services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();

        return services;
    }
}