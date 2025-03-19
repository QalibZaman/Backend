using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications;

public class GetCategoriesByNameOrUrlSpecification : Specification<CategoryEntity>
{
    public GetCategoriesByNameOrUrlSpecification(string name, string url, int shopId)
            : base(category => category.ShopId == shopId && (category.Name == name || category.URL == url )) { }
}
