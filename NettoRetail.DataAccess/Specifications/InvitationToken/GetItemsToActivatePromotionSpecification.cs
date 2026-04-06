using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.InvitationToken;

public class GetItemsToDeactivatePromotionSpecification : Specification<InventoryItemEntity>
{
    public GetItemsToDeactivatePromotionSpecification(DateOnly today)
        : base(item =>
            item.PromotionEndDate != null &&
            item.PromotionEndDate < today &&
            item.IsPromoted == true)
    {
    }
}