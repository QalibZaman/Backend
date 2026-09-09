using Mapster;
using NettoRetail.Application.Models.Response.Category;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.MappingConfiguration;

public class CategoryMappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CategoryEntity, CategoryResponse>()
            .Map(dest => dest.ParentName, src => src.ParentCategory != null ? src.ParentCategory.Name : null)
            // TODO: когда появится InventoryItemEntity — заменить на реальный Count().
            .Map(dest => dest.ItemsAmount, src => 0);
    }
}