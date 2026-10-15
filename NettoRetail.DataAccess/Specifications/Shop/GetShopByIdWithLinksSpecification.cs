using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Shop
{
    public class GetShopByIdWithLinksSpecification : Specification<ShopEntity>
    {
        public GetShopByIdWithLinksSpecification(int id)
            : base(shop => shop.Id == id)
        {
            AddInclude(shop => shop.ShopLinks);
        }
    }
}
