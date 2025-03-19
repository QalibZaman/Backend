using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;
public class GetInventoryItemsByMaxReservedAmountSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemsByMaxReservedAmountSpecification(int maxAmount)
        : base(x => x.ReservedAmount <= maxAmount) { }
}