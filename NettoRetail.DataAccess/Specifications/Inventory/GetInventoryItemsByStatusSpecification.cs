using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetInventoryItemsByStatusSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemsByStatusSpecification(InventoryItemStatusEnum status)
        : base(x => x.Status == status) { }
}
