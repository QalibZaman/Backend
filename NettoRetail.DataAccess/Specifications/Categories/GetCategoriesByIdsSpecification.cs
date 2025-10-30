using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Categories;

public class GetCategoriesByIdsSpecification : Specification<CategoryEntity>
{
    public GetCategoriesByIdsSpecification(List<int> ids)
            : base(x => ids.Contains(x.Id)) { }

    public GetCategoriesByIdsSpecification(int shopId)
        : base(x => x.ShopId == shopId) { }
}