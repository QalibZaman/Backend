using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory
{
    public class GetInventoryItemWithChildrenSpecification : Specification<InventoryItemEntity>
    {
        public GetInventoryItemWithChildrenSpecification(Guid itemId)
            : base(x => x.Id == itemId)
        {
            AddInclude(i => i.ChildItems!);
        }
    }
}
