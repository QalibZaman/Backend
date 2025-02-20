using Mapster;
using NettoHealth.Application.Models.Entities;
using NettoHealth.Domain.Entities;
using NettoHealth.Shared.Utilities;

namespace NettoHealth.Application.MappingConfiguration;

public class MappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<HealthcareProviderRegistrationModel, HealthcareProviderEntity>()
            .Map(dest => dest.PasswordHash, src => src.Password);
    }
}