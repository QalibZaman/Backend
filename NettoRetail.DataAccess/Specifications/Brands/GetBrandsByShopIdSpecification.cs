using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Brands;

public class GetBrandsByShopIdSpecification : Specification<BrandEntity>
{
    public GetBrandsByShopIdSpecification(int shopId)
    : base(b => b.ShopId == shopId) { }
}
