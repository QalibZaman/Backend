using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Categories;

public class GetCategoryByIdAndShopIdSpecification(int id, int shopId) : Specification<CategoryEntity>(category => category.Id == id && category.ShopId == shopId)
{
}