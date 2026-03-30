using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Categories;

public class GetVisibleLevelOneCategoriesByShopIdSpecification : Specification<CategoryEntity>
{
    public GetVisibleLevelOneCategoriesByShopIdSpecification(int shopId)
        : base(category => category.ShopId == shopId && category.Visibility && category.Level == 1)
    {
    }
}
