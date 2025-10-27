using System.Linq.Expressions;
using Mapster;
using MediatR;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.IInventoryItemQueryService;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries;

public record GetInventoryItemsQuery(
    int ShopId,
    InventoryItemsSearchModel Model) : IRequest<PaginatedResponse<InventoryItemListResponse>>;

public class GetInventoryItemsQueryHandler
    : IRequestHandler<GetInventoryItemsQuery, PaginatedResponse<InventoryItemListResponse>>
{
    private readonly IInventoryItemQueryService _queryService;

    public GetInventoryItemsQueryHandler(IInventoryItemQueryService listingService)
    {
        _queryService = listingService;
    }

    public async Task<PaginatedResponse<InventoryItemListResponse>> Handle(
        GetInventoryItemsQuery request,
        CancellationToken cancellationToken)
    {
        return await _queryService.GetInventoryItemsAsync(
            request.ShopId,
            request.Model,
            cancellationToken);
    }
}
