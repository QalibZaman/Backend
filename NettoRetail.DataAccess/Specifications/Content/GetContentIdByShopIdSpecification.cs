using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Content;

public class GetContentBlockByShopIdSpecification : Specification<ContentEntity>
{
    public GetContentBlockByShopIdSpecification(int id, int shopId)
        : base(c => c.Id == id && c.ShopId == shopId)
    {
    }
}
