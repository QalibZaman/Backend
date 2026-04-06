using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetItemsToActivatePromotionSpecification : Specification<InventoryItemEntity>
{
    public GetItemsToActivatePromotionSpecification(DateOnly today)
        : base(item =>
            item.PromotionStartDate != null &&
            item.PromotionEndDate != null &&
            item.PromotionStartDate <= today &&
            item.PromotionEndDate >= today &&
            item.IsPromoted == false)
    {
    }
}