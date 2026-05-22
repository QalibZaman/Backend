using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.ProductReview
{
    public class IsVerifiedPurchaseSpecification : Specification<OrderEntity>
    {
        public IsVerifiedPurchaseSpecification(Guid customerId, Guid inventoryItemId)
            : base(order => order.CustomerId == customerId && order.OrderItems.Any(oi => oi.InventoryItemId == inventoryItemId)) 
        {

        }
    }
}
