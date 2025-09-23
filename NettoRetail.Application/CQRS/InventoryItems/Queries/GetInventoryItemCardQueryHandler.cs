using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries;

public record GetInventoryItemCardsQuery(
        int ShopId,
        int? CategoryId,
        string? Search,
        PaginationRequest Paginated) : IRequest<PaginatedResponse<InventoryItemCardResponse>>;
public class GetInventoryItemCardQueryHandler(IInventoryItemRepository inventoryItemRepository) : IRequestHandler<GetInventoryItemCardsQuery, PaginatedResponse<InventoryItemCardResponse>>
{
    private const string Placeholder = ImagePlaceholdersConstants.InventoryItem;

    public async Task<PaginatedResponse<InventoryItemCardResponse>> Handle(GetInventoryItemCardsQuery request, CancellationToken cancellationToken)
    {
        var spec = new SpecificationBuilder<InventoryItemEntity>()
        .Add<GetInventoryItemsByShopIdSpecification>(request.ShopId)
        .Add<GetInventoryItemsByCategoriesSpecification>(new List<int> { request.CategoryId.Value })
        .Add<GetInventoryItemsByPartialNameSpecification>(request.Search)
        .Skip((request.Paginated.PageNumber - 1) * request.Paginated.PageSize)
        .Take(request.Paginated.PageSize)
        .Build();

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
