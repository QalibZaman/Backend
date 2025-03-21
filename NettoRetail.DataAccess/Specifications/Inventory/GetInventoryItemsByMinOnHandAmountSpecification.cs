using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetInventoryItemsByMinOnHandAmountSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemsByMinOnHandAmountSpecification(int minAmount)
        : base(x => x.OnHandAmount >= minAmount) { }
}