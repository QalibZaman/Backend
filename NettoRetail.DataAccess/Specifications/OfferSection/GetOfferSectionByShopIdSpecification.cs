using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.OfferSection;

public class GetOfferSectionByShopIdSpecification : Specification<OfferEntity>
{
    public GetOfferSectionByShopIdSpecification(int shopId)
        : base(x => x.ShopId == shopId)
    {
        AddInclude(x => x.Products);
    }
}