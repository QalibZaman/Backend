using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.TeaserSection;

public class GetTeaserSectionByIdAndShopIdSpecification : Specification<TeaserSectionEntity>
{
    public GetTeaserSectionByIdAndShopIdSpecification(Guid id, int shopId)
        : base(x => x.Id == id && x.ShopId == shopId) { }
}