using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;
public class GetInventoryItemsByMinReservedAmountSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemsByMinReservedAmountSpecification(int minAmount)
        : base(x => x.ReservedAmount >= minAmount) { }
}