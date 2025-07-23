using System.Linq.Expressions;
using Mapster;
using MediatR;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries;

public record GetInventoryItemsSortedByPriceQuery(int ShopId, InventoryItemSortingEnum SortOption) : IRequest<List<InventoryItemListResponse>>;

public class GetSortedInventoryItemsQueryHandler(IInventoryItemRepository inventoryItemRepository) : IRequestHandler<GetInventoryItemsSortedByPriceQuery, List<InventoryItemListResponse>>
{
    public async Task<List<InventoryItemListResponse>> Handle(GetInventoryItemsSortedByPriceQuery request, CancellationToken cancellationToken)
    {
        var specification = new GetInventoryItemPriceSortingSpecification(request.ShopId, request.SortOption);
        var items = await inventoryItemRepository.GetAllBySortedAsync(specification, cancellationToken);
        return items.Adapt<List<InventoryItemListResponse>>();
    }
}