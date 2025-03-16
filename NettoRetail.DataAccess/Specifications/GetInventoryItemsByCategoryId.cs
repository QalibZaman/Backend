using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications;

public class GetInventoryItemsByCategoryId : Specification<InventoryItemEntity>
{
    public GetInventoryItemsByCategoryId(int id)
        : base(x => x.CategoryId == id) { }
}