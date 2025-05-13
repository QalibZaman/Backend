using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Shop;

public class GetAllShopsSpecification : Specification<ShopEntity>
{
    public GetAllShopsSpecification()
        : base(x => true)
    {
        AddSelectExpression(shop => new
        {
            shop.Id,
            shop.Name,
            shop.Subscription,
        });
    }
}