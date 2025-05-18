
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Content;

public class GetContentBlockByIdSpecification : Specification<ContentEntity>
{
    public GetContentBlockByIdSpecification(int id, int shopId)
        : base(c => c.Id == id && c.ShopId == shopId)
    {
    }
}
