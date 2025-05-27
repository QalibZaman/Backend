using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetInventoryItemWithImageByIdSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemWithImageByIdSpecification(Guid id)
        : base(x => x.Id == id)
    {
        AddInclude(x => x.Images);
    }
}
