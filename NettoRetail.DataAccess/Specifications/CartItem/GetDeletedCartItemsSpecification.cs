using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.CartItem;

public class GetDeletedCartItemsSpecification : Specification<CartItemEntity>
{
    public GetDeletedCartItemsSpecification(Guid cartId)
    : base(x => x.CartId == cartId && x.IsDeleted == true) { }
}
