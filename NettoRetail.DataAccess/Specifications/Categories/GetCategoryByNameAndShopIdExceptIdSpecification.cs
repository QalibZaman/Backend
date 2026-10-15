using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Categories
{
    public class GetCategoryByNameAndShopIdExceptIdSpecification(
    string name,
    int categoryId,
    int shopId)
    : Specification<CategoryEntity>(
        category =>
            category.ShopId == shopId &&
            category.Id != categoryId &&
            category.Name.ToLower() == name.ToLower())
    {
    }
}
