using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;
public class GetItemByIdSpecification : Specification<InventoryItemEntity>
{
    public GetItemByIdSpecification(Guid id)
        : base(x => x.Id == id) { }
}