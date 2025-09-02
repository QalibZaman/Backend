using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.ProductBanner
{
    public class InventoryItemBannerByIdSpecification : Specification<InventoryItemBannerEntity>
    {
        public InventoryItemBannerByIdSpecification(Guid id)
            : base(pb => pb.Id == id) { }
    }
}
