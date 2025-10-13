using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.CartItem;

public class GetCartItemByCartAndItemIdSpecification : Specification<CartItemEntity>
{
    public GetCartItemByCartAndItemIdSpecification(Guid cartId, Guid inventoryItemId)
    : base(x => x.CartId == cartId && x.InventoryItemId == inventoryItemId && x.IsDeleted == false) { }
}
