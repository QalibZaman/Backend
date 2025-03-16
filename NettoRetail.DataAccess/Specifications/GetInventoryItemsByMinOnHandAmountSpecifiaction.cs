using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications;

public class GetInventoryItemsByMinOnHandAmountSpecifiaction : Specification<InventoryItemEntity>
{
    public GetInventoryItemsByMinOnHandAmountSpecifiaction(int minAmount)
        : base(x => x.OnHandAmount >= minAmount) { }
}