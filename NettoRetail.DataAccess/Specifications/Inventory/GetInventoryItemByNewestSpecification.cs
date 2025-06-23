using System.Linq.Expressions;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetInventoryItemByNewestSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemByNewestSpecification()
        : base(Specification<InventoryItemEntity>.Empty)
    {
        AddOrderBy(x => x.LastUpdatedDate, descending: true);
    }
}