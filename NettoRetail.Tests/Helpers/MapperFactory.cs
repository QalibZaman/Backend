using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using NettoRetail.Application.MappingConfiguration;

namespace NettoRetail.Tests.Helpers;

internal static class MapperFactory
{
    public static IMapper Create()
    {
        var config = new TypeAdapterConfig();
        config.Apply(new MappingConfiguration());

        var services = new ServiceCollection();
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IMapper>();
    }
}
