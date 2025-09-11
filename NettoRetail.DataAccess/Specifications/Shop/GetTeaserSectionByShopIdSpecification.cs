using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

public class GetTeaserSectionByShopIdSpecification : Specification<TeaserSectionEntity>
{
    public GetTeaserSectionByShopIdSpecification(int shopId)
        : base(x => x.ShopId == shopId) { }
}
