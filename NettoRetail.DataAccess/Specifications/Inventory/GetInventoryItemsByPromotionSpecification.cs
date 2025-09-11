using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetInventoryItemsByPromotionSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemsByPromotionSpecification(bool isPromoted)
        : base(x => x.IsPromoted == isPromoted) { }
}
