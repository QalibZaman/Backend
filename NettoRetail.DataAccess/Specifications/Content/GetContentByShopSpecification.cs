using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;
namespace NettoRetail.DataAccess.Specifications.Content;
public class GetContentByShopSpecification : Specification<ContentEntity>
{
    public GetContentByShopSpecification(int shopId)
        : base(c => c.ShopId == shopId)
    {
        AddOrderBy(c => c.SectionType);
    }
}
