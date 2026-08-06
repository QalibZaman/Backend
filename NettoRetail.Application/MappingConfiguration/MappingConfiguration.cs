using Mapster;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.MappingConfiguration;

public class MappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<NotificationEntity, NotificationResponse>();
    }
}
