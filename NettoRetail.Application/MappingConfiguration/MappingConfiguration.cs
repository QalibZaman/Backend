using Mapster;
using NettoRetail.Application.Models.Admin;
using NettoRetail.Application.Models.Category;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.MappingConfiguration;

public class MappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AdminSignUpModel, StaffEntity>()
            .Map(dest => dest.PasswordHash, src => src.Password);

        config.NewConfig<CategoryEntity, CategoryResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.ParentId, src => src.ParentId)
            .Map(dest => dest.Level, src => src.Level)
            .Map(dest => dest.URL, src => src.URL)
            .Map(dest => dest.Visibility, src => src.Visibility)
            .Map(dest => dest.ItemsAmount, src => src.InventoryItems.Count());
    }
}