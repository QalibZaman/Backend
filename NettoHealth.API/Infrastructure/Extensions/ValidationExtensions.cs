using FluentValidation;
using FluentValidation.AspNetCore;
using NettoHealth.API.Infrastructure.Validators;

namespace NettoHealth.API.Infrastructure.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection ConfigureValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<HealthcareProviderRegistrationValidator>();
        services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();

        return services;
    }
}