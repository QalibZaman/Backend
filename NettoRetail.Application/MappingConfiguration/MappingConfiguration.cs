using Mapster;
using NettoRetail.Application.Models.Admin;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;

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

        config.NewConfig<CustomerEntity, CustomerResponse>()
            .Map(dest => dest.FirstName, src => src.Account.FirstName)
            .Map(dest => dest.LastName, src => src.Account.LastName)
            .Map(dest => dest.Email, src => src.Account.Email);

        config.NewConfig<UpdateInventoryItemModel, InventoryItemEntity>()
            .IgnoreNullValues(true);

        config.NewConfig<ProductDetailsModel, ProductDetailsEntity>()
            .Map(dest => dest.TypeEnum, src => Enum.Parse<ProductDetailTypeEnum>(src.Type))
            .Map(dest => dest.Unit, src =>
                string.IsNullOrEmpty(src.Unit) ? null : (ProductDetailUnitEnum?)Enum.Parse<ProductDetailUnitEnum>(src.Unit));

        config.NewConfig<ProductDetailsEntity, ProductDetailsResponse>()
            .Map(dest => dest.Type, src => src.TypeEnum.ToString());
    }
}