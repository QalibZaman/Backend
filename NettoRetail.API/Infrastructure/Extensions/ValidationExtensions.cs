using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Models.Response;

namespace NettoRetail.API.Infrastructure.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection ConfigureValidation(this IServiceCollection services)
    {
        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .ToDictionary(
                            e => e.Key.ToLowerInvariant(),
                            e => e.Value!.Errors.Select(err => err.ErrorMessage)
                                .ToArray());

                    var response = new ApiResponse("The given data does not follow validation rules", errors);

                    return new BadRequestObjectResult(response);
                };
            });

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        //services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();

        return services;
    }
}