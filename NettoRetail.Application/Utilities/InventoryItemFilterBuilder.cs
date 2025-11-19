using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.Utilities;
public static class InventoryItemFilterBuilder
{
    public static Specification<InventoryItemEntity> Build(GetFilteredInventoryItemsModel filter, int shopId, IEnumerable<int>? categoryIds)
    {
        var builder = new SpecificationBuilder<InventoryItemEntity>();

        builder
            .Add<GetInventoryItemsByShopIdSpecification>(shopId)
            .Add<GetInventoryItemsByPartialNameSpecification>(filter.ProductName)
            .Add<GetInventoryItemsByPartialLocationSpecification>(filter.ProductLocation)
            .Add<GetInventoryItemsByMinAvailableAmountSpecification>(filter.MinAvailableAmount)
            .Add<GetInventoryItemsByMinReservedAmountSpecification>(filter.MinReservedAmount)
            .Add<GetInventoryItemsByMinOnHandAmountSpecification>(filter.MinOnHandAmount)
            .Add<GetInventoryItemsByMinPriceSpecification>(filter.MinPrice);

        if (filter.MaxPrice != null)
            builder.Add<GetInventoryItemsByMaxPriceSpecification>(filter.MaxPrice);

        if (filter.MaxAvailableAmount != null)
            builder.Add<GetInventoryItemsByMaxAvailableAmountSpecification>(filter.MaxAvailableAmount);

        if (filter.MaxReservedAmount != null)
            builder.Add<GetInventoryItemsByMaxReservedAmountSpecification>(filter.MaxReservedAmount);

        if (filter.MaxOnHandAmount != null)
            builder.Add<GetInventoryItemsByMaxOnHandAmountSpecification>(filter.MaxOnHandAmount);

        if (categoryIds?.Any() == true)
            builder.Add<GetInventoryItemsByCategoriesSpecification>(categoryIds.ToList());

        if (!string.IsNullOrEmpty(filter.Status)
            && Enum.TryParse(filter.Status, true, out InventoryItemStatusEnum status))
            builder.Add<GetInventoryItemsByStatusSpecification>(status);

        if (!string.IsNullOrEmpty(filter.BrandName))
            builder.Add<GetInventoryItemsByBrandsPartialNameSpecification>(filter.BrandName);

        return builder.Build();
    }
}
