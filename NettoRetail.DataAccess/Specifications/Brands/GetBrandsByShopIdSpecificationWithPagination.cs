using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Brands;

public class GetBrandsByShopIdSpecificationWithPagination : Specification<BrandEntity>
{
    public GetBrandsByShopIdSpecificationWithPagination(int shopId, int skip, int take)
        : base(b => b.ShopId == shopId)
    {
        AddOrderBy(b => b.Name);
        AddOrderBy(b => b.Id);
        Skip = skip;
        Take = take;
    }
}
