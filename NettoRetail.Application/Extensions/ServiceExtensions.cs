using System.Reflection;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Services;

namespace NettoRetail.Application.Extensions;

public static class ServiceExtensions
{
    public static void AddApplicationServiceExtensions(this IServiceCollection services)
    {
        // Mediator
        services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        //Mapping
        services.AddMapster();
        TypeAdapterConfig.GlobalSettings.Apply(new MappingConfiguration.MappingConfiguration());

        //Services
        services.AddScoped<ICategoryLevelManagementService, CategoryLevelManagementService>();
        services.AddScoped<IJwtManager, JwtManager>();
    }
}