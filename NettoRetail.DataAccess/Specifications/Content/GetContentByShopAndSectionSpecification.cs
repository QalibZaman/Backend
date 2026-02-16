using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Content;

public class GetContentByShopAndSectionSpecification : Specification<ContentEntity>
{
    public GetContentByShopAndSectionSpecification(int shopId, SectionTypeEnum sectionType)
        : base(c => c.ShopId == shopId && c.SectionType == sectionType)
    {
    }
}
