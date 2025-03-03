using System.Reflection;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.Application.Services.ISessionCleanupService;
using NettoRetail.Application.Services.ISessionModelFactory;

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

        //Background
        services.AddHostedService<SessionCleanupService>();

        //Other
        services.AddScoped<ISessionModelFactory, SessionModelFactory>();
    }
}