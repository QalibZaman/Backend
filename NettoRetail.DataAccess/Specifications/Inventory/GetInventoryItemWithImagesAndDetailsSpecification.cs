using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory
{
    public class GetInventoryItemWithImagesAndDetailsSpecification : Specification<InventoryItemEntity>
    {
        public GetInventoryItemWithImagesAndDetailsSpecification(Guid id, int shopId)
            : base(x => x.Id == id && x.ShopId == shopId)
        {
            AddInclude(x => x.Images);
            AddInclude(x => x.ProductDetails);
        }
    }
}
