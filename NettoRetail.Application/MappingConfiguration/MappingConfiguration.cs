using Mapster;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Roles;
using NettoRetail.Application.Utilities;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Application.MappingConfiguration;

public class MappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CustomerEntity, GetPersonalInformationResponse>()
            .Map(dest => dest.CountryId,
                src => src.Address == null ? null : src.Address.CountryId)
            .Map(dest => dest.Street,
                src => src.Address == null ? null : src.Address.Street)
            .Map(dest => dest.HouseNumber,
                src => src.Address == null ? null : src.Address.HouseNumber)
            .Map(dest => dest.Apartment,
                src => src.Address == null ? null : src.Address.Apartment)
            .Map(dest => dest.City,
                src => src.Address == null ? null : src.Address.City)
            .Map(dest => dest.State,
                src => src.Address == null ? null : src.Address.State)
            .Map(dest => dest.ZipCode,
                src => src.Address == null ? null : src.Address.ZipCode);

        config.NewConfig<StaffEntity, GetPersonalInformationResponse>()
            .Map(dest => dest.CountryId,
                src => src.Address == null ? null : src.Address.CountryId)
            .Map(dest => dest.Street,
                src => src.Address == null ? null : src.Address.Street)
            .Map(dest => dest.HouseNumber,
                src => src.Address == null ? null : src.Address.HouseNumber)
            .Map(dest => dest.Apartment,
                src => src.Address == null ? null : src.Address.Apartment)
            .Map(dest => dest.City,
                src => src.Address == null ? null : src.Address.City)
            .Map(dest => dest.State,
                src => src.Address == null ? null : src.Address.State)
            .Map(dest => dest.ZipCode,
                src => src.Address == null ? null : src.Address.ZipCode);

        config.NewConfig<UpdatePersonalInformationRequest, CustomerEntity>()
            .IgnoreNullValues(true);

        config.NewConfig<UpdatePersonalInformationRequest, StaffEntity>()
            .IgnoreNullValues(true);

        config.NewConfig<SubscriptionPlanEntity, SubscriptionPlanResponce>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Motto, src => src.Motto)
            .Map(dest => dest.MonthlyPrice, src => src.MonthlyPrice);

        config.NewConfig<UpdatePersonalInformationRequest, AddressEntity>()
            .IgnoreNullValues(true);

        config.NewConfig<RoleEntity, GetRolePermissionsResponse>()
            .Map(dest => dest.RoleId, src => src.Id)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.RoleName, src => src.Name)
            .AfterMapping((src, dest) =>
            {
                var rolePermissionTitles = src.PermissionRoles
                    .Where(pr => pr.Permission != null)
                    .Select(pr => pr.Permission!.Title)
                    .ToHashSet();

                foreach (var permission in PermissionProvider.All)
                {
                    dest.Permissions[permission] = rolePermissionTitles.Contains(permission);
                }
            });
    }
}