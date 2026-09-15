using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Categories;

public class GetCategoryByNameSpecification(string name, int shopId) : Specification<CategoryEntity>(category => category.ShopId == shopId && category.Name.ToLower() == name.ToLower())
{
}