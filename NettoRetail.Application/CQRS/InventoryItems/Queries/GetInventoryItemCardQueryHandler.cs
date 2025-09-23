using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries;

public record GetInventoryItemCardsQuery(
        int ShopId,
        int? CategoryId,
        string? Search,
        int Page,
        int PageSize) : IRequest<PaginatedResponse<InventoryItemCardResponse>>;
public class GetInventoryItemCardQueryHandler(IInventoryItemRepository inventoryItemRepository) : IRequestHandler<GetInventoryItemCardsQuery, PaginatedResponse<InventoryItemCardResponse>>
{
    private const string Placeholder = "/images/placeholder.png";

    public async Task<PaginatedResponse<InventoryItemCardResponse>> Handle(GetInventoryItemCardsQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetInventoryItemCardSpecification(
        shopId: request.ShopId,
        categoryId: request.CategoryId,
        search: request.Search,
        page: request.Page,
        pageSize: request.PageSize);

        var (items, total) = await inventoryItemRepository.GetAllPaginatedAsync(spec, cancellationToken);

        var cards = items.Select(i =>
        {
            var imgs = (i.Images ?? []).OrderBy(im => im.Order).ToList();

            return new InventoryItemCardResponse
            {
                Id = i.Id,
                ProductName = i.ProductName,
                CategoryName = i.Category?.Name ?? string.Empty,
                MediaUrls = imgs.Select(im => im.BlobUrl),
                ThumbnailUrl = imgs.FirstOrDefault()?.BlobUrl ?? Placeholder,
                Price = i.Price,
                PromotionPrice = i.PromotionPrice,
                AvailableAmount = i.AvailableAmount,
                IsOnSale = i.PromotionPrice.HasValue,
                IsNew = i.LastUpdatedDate >= DateTime.UtcNow.AddDays(-7),
                IsOutOfStock = i.AvailableAmount <= 0
            };

        }).ToList();

        return new PaginatedResponse<InventoryItemCardResponse>
        {
            Items = cards,
            TotalAmount = total
        };
    }
}
