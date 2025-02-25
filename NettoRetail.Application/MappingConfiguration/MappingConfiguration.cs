using Mapster;
using NettoRetail.Application.Models.Admin;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.MappingConfiguration;

public class MappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AdminSignUpModel, StaffEntity>()
            .Map(dest => dest.PasswordHash, src => src.Password);
    }
}