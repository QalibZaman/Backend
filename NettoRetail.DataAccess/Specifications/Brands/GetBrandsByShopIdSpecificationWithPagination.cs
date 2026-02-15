using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Brands;

public class GetBrandsByShopIdSpecificationWithPagination : Specification<BrandEntity>
{
    public GetBrandsByShopIdSpecificationWithPagination(int shopId, int pageNumber, int pageSize)
        : base(b => b.ShopId == shopId)
    {
        AddOrderBy(b => b.Name);
        AddOrderBy(b => b.Id);
        ApplyPaging(pageNumber, pageSize);
    }
}
