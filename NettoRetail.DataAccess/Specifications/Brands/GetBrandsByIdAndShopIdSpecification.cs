using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Brands;

public class GetBrandsByIdAndShopIdSpecification : Specification<BrandEntity>
{
    public GetBrandsByIdAndShopIdSpecification(int id, int shopId)
    : base(b => b.Id == id && b.ShopId == shopId) { }
}
