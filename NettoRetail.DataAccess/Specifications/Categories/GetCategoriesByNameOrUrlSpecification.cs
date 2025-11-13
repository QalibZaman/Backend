using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Categories;

public class GetCategoriesByNameOrUrlSpecification : Specification<CategoryEntity>
{
    public GetCategoriesByNameOrUrlSpecification(string name, string url, int shopId)
            : base(category => category.ShopId == shopId && (category.Name.Trim().ToLower() == name.Trim().ToLower() || category.URL == url)) { }
}