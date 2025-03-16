using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications;

public class GetInventoryItemsByMaxOnHandAmountSpecifiaction : Specification<InventoryItemEntity>
{
    public GetInventoryItemsByMaxOnHandAmountSpecifiaction(int maxAmount)
        : base(x => x.OnHandAmount <= maxAmount) { }
}