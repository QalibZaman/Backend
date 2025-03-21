using Mapster;
using NettoRetail.Application.Models.Admin;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.MappingConfiguration;

public class MappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AdminSignUpModel, StaffEntity>()
            .Map(dest => dest.PasswordHash, src => src.Password);

        config.NewConfig<CategoryEntity, CategoryResponse>()
            .Map(dest => dest.ItemsAmount, src => src.InventoryItems.Count());

        config.NewConfig<InventoryItemEntity, InventoryItemCsvModel>()
            .Map(dest => dest.ProductCategory, src => src.Category.Name);
    }
}