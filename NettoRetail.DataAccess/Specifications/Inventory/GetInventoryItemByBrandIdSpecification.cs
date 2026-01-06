using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetInventoryItemByBrandIdSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemByBrandIdSpecification(int brandId)
        : base(x => x.BrandId == brandId) { }
}
