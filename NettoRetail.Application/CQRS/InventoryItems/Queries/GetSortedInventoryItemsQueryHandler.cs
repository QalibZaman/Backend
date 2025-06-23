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

public record GetInventoryItemsSortedByPriceQuery(GetSortedInventoryItemsModel Model) : IRequest<List<InventoryItemListResponse>>;

public class GetSortedInventoryItemsQueryHandler(IInventoryItemRepository inventoryItemRepository) : IRequestHandler<GetInventoryItemsSortedByPriceQuery, List<InventoryItemListResponse>>
{
    public async Task<List<InventoryItemListResponse>> Handle(GetInventoryItemsSortedByPriceQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<InventoryItemEntity, object>> sortExpr;
        bool isDesc;

        switch(request.Model.SortOption)
        {
            case InventoryItemSortingEnum.PriceLowToHigh:
                sortExpr = x => x.Price;
                isDesc = false;
                break;
            case InventoryItemSortingEnum.PriceHighToLow:
                sortExpr = x => x.Price;
                isDesc = true;
                break;
            case InventoryItemSortingEnum.Newest:
                sortExpr = x => x.LastUpdatedDate;
                isDesc = true;
                break;
            default:
                sortExpr = x => x.ProductName;
                isDesc = false;
                break;
        }


        var specification = new Specification<InventoryItemEntity>(x => x.ShopId == request.Model.ShopId)
            .AddOrderBy(sortExpr, isDesc);

        var items = await inventoryItemRepository.GetAllBySortedAsync(specification, cancellationToken);
        return items.Adapt<List<InventoryItemListResponse>>();
    }
}