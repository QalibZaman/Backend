using Mapster;
using NettoRetail.Application.Models.Admin;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.MappingConfiguration;

public class MappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AdminSignUpModel, AccountEntity>()
            .Map(dest => dest.PasswordHash, src => src.Password);

        config.NewConfig<CategoryEntity, CategoryResponse>()
            .Map(dest => dest.ItemsAmount, src => src.InventoryItems.Count());

        config.NewConfig<InventoryItemEntity, InventoryItemListResponse>()
            .Map(dest => dest.ProductCategory, src => src.Category.Name);

        config.NewConfig<RoleEntity, RoleResponse>()
            .Map(dest => dest.UserCount, src => src.Accounts.Count());

        config.NewConfig<InventoryItemEntity, InventoryItemCsvModel>()
            .Map(dest => dest.ProductCategory, src => src.Category.Name);
    }
}