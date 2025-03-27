using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Categories;

public class GetCategoriesByShopIdSpecification : Specification<CategoryEntity>
{
    public GetCategoriesByShopIdSpecification(int shopId)
        : base(model => model.ShopId == shopId) { }
}
