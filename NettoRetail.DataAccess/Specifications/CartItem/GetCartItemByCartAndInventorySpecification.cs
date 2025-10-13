using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.CartItem;

public class GetCartItemByCartAndInventorySpecification : Specification<CartItemEntity>
{
    public GetCartItemByCartAndInventorySpecification(Guid cartId, Guid inventoryItemId, string? size)
        : base(x => x.CartId == cartId
                 && x.InventoryItemId == inventoryItemId
                 && x.Size == size)
    {
    }
}
