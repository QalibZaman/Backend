using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Shop;

public class GetShopByEmailSpecification : Specification<ShopEntity>
{
    public GetShopByEmailSpecification(string email)
    : base(x => x.Email == email) { }
}
