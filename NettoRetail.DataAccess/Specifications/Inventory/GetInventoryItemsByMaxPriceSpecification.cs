using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetInventoryItemsByMaxPriceSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemsByMaxPriceSpecification(decimal maxPrice)
        : base(x => x.Price <= maxPrice) { }
}
