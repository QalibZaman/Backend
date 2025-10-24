using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Categories;

public class GetCategoryByIdAndShopIdSpecification : Specification<CategoryEntity>
{
    public GetCategoryByIdAndShopIdSpecification(int id, int shopId)
            : base(category => category.Id == id && category.ShopId == shopId) { }
}
