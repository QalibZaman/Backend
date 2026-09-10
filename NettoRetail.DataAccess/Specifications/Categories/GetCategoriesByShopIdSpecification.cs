using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Categories;

public class GetCategoriesByShopIdSpecification(int shopId) : Specification<CategoryEntity>(category => category.ShopId == shopId)
{
}