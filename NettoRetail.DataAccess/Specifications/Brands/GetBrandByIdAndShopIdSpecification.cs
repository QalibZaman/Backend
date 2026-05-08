using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Brands;

public class GetBrandByIdAndShopIdSpecification : Specification<BrandEntity>
{
    public GetBrandByIdAndShopIdSpecification(int id, int shopId)
    : base(b => b.Id == id && b.ShopId == shopId) { }
}
