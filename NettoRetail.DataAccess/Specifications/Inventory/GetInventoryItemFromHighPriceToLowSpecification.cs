using System.Linq.Expressions;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetInventoryItemFromHighPriceToLowSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemFromHighPriceToLowSpecification()
        : base(Specification<InventoryItemEntity>.Empty)
    {
        AddOrderBy(x => x.Price, descending: true);
    }
}