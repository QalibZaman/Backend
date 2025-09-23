using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.CartItem;

public class GetCartItemsByCartIdSpecification : Specification<CartItemEntity>
{
    public GetCartItemsByCartIdSpecification(Guid cartId)
    : base(x => x.CartId == cartId) { }
}
