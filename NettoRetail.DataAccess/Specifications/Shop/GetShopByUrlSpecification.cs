using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Shop;

public class GetShopByUrlSpecification : Specification<ShopEntity>
{
    public GetShopByUrlSpecification(string url)
        : base(x => x.Url == url) { }
}
