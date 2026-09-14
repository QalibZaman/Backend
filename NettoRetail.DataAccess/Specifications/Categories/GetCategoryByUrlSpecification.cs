using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Categories;

public class GetCategoryByUrlSpecification(string url, int shopId) : Specification<CategoryEntity>(category => category.ShopId == shopId && category.URL.ToLower() == url.ToLower())
{
}