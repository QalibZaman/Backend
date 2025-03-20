using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetInventoryItemByIdSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemByIdSpecification(Guid id)
        : base(x => x.Id == id) { }
}
