using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Cart;

public class GetCartByIdSpecification : Specification<CartEntity>
{
    public GetCartByIdSpecification(Guid cartId)
    : base(x => x.Id == cartId) { }
}
