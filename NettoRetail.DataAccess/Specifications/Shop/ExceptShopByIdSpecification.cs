using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Shop;

public class ExceptShopByIdSpecification : Specification<ShopEntity>
{
    public ExceptShopByIdSpecification(int id)
        : base (x => x.Id != id)
    { }
}
