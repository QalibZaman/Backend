using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Shop;

public class GetShopByNameSpecification : Specification<ShopEntity>
{
    public GetShopByNameSpecification(string name)
    : base(x => x.Name == name) { }
}
