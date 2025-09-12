using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetInventoryItemsByMinPriceSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemsByMinPriceSpecification(decimal minPrice)
        : base(x => x.IsPromoted == true ? x.PromotionPrice >= minPrice : x.Price >= minPrice) { }
}
