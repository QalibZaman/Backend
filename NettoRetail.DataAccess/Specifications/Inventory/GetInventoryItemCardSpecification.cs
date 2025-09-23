using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Inventory;

public class GetInventoryItemCardSpecification : Specification<InventoryItemEntity>
{
    public GetInventoryItemCardSpecification(
        int shopId,
        int? categoryId,
        string? search,
        int page,
        int pageSize) : base(x => x.IsActive && x.ShopId == shopId)
    {
        if (categoryId.HasValue)

            AddCriteria(x => x.CategoryId == categoryId.Value);

        if (!string.IsNullOrEmpty(search))
            AddCriteria(x => x.ProductName.Contains(search) || x.ProductUpc.ToString().Contains(search));

        AddInclude(x => x.Images);
        AddInclude(x => x.Category);

        AddOrderBy(x => x.LastUpdatedDate, descending: true);

        Skip = (page - 1) * pageSize;
        Take = pageSize;
    }
}
