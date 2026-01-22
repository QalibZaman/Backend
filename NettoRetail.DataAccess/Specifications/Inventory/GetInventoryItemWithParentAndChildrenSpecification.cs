using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory
{
    public class GetInventoryItemWithParentAndChildrenSpecification : Specification<InventoryItemEntity>
    {
        public GetInventoryItemWithParentAndChildrenSpecification(Guid itemId)
            : base(x => x.Id == itemId)
        {
            AddInclude(i => i.ParentItem!);
            AddInclude(i => i.ChildItems!);
        }
    }
}
