using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Brands;

public class GetBrandByIdShopIdSpecification : Specification<BrandEntity>
{
    public GetBrandByIdShopIdSpecification(int id, int shopId)
        : base(model => model.Id == id && model.ShopId == shopId) { }
}
