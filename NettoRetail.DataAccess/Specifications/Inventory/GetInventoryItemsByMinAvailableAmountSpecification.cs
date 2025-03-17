using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;
public class GetInventoryItemsByMinAvailableAmountSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemsByMinAvailableAmountSpecification(int minAmount)
        : base(x => x.AvailableAmount >= minAmount) { }
}