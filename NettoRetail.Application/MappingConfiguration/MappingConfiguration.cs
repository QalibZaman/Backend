using Mapster;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.MappingConfiguration;

public class MappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CustomerEntity, GetPersonalInformationResponse>()
            .Map(dest => dest.CountryId, src => src.Address == null ? null : src.Address.CountryId)
            .Map(dest => dest.Street, src => src.Address == null ? null : src.Address.Street)
            .Map(dest => dest.HouseNumber, src => src.Address == null ? null : src.Address.HouseNumber)
            .Map(dest => dest.Apartment, src => src.Address == null ? null : src.Address.Apartment)
            .Map(dest => dest.City, src => src.Address == null ? null : src.Address.City)
            .Map(dest => dest.State, src => src.Address == null ? null : src.Address.State)
            .Map(dest => dest.ZipCode, src => src.Address == null ? null : src.Address.ZipCode);

        config.NewConfig<StaffEntity, GetPersonalInformationResponse>()
            .Map(dest => dest.CountryId, src => src.Address == null ? null : src.Address.CountryId)
            .Map(dest => dest.Street, src => src.Address == null ? null : src.Address.Street)
            .Map(dest => dest.HouseNumber, src => src.Address == null ? null : src.Address.HouseNumber)
            .Map(dest => dest.Apartment, src => src.Address == null ? null : src.Address.Apartment)
            .Map(dest => dest.City, src => src.Address == null ? null : src.Address.City)
            .Map(dest => dest.State, src => src.Address == null ? null : src.Address.State)
            .Map(dest => dest.ZipCode, src => src.Address == null ? null : src.Address.ZipCode);

        config.NewConfig<UpdatePersonalInformationRequest, CustomerEntity>();

        config.NewConfig<UpdatePersonalInformationRequest, StaffEntity>();

        config.NewConfig<UpdatePersonalInformationRequest, AddressEntity>();
    }
}
