using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Shops.Queries;

public record GetShopProductDetailsQuery(int ShopId, Guid ProductId) : IRequest<InventoryItemDetailsResponse>;

public class GetShopProductDetailsQueryHandler(
    IShopRepository shopRepository,
    IInventoryItemRepository itemsRepository,
    ICategoryRepository categoryRepository)
    : IRequestHandler<GetShopProductDetailsQuery, InventoryItemDetailsResponse>
{
    public async Task<InventoryItemDetailsResponse> Handle(GetShopProductDetailsQuery request, CancellationToken cancellationToken)
    {
        var shopSpec = new GetShopByIdSpecification(request.ShopId);
        var isShopExists = await shopRepository.Exists(shopSpec, cancellationToken);
        if (!isShopExists)
            throw new NotFoundException("There is no shop with such id.");

        var itemSpec = new SpecificationBuilder<InventoryItemEntity>()
            .Add<GetInventoryItemsByShopIdSpecification>(request.ShopId)
            .Add<GetItemByIdSpecification>(request.ProductId)
            .AddInclude(i => i.Category)
            .AddInclude(i => i.Images)
            .Build();

        var item = await itemsRepository.GetAsync(itemSpec, cancellationToken) ?? throw new NotFoundException("There is no product with such id.");

        var categories = await categoryRepository.GetCategoryWithHierarchyAsync(item.CategoryId, cancellationToken);

        var response = new InventoryItemDetailsResponse
        {
            Id = item.Id,
            ProductName = item.ProductName,
            Description = item.Description,
            Categories = categories.Select(c => c.Name),
            MediaUrls = item.Images.OrderBy(i => i.Order).Select(i => i.BlobUrl),
            Price = item.Price,
            PromotionPrice = item.PromotionPrice,
            AvailableAmount = item.AvailableAmount,
            LastUpdatedDate = item.LastUpdatedDate,
            IsOnSale = item.PromotionPrice.HasValue,
            IsOutOfStock = item.AvailableAmount <= 0,
            IsNew = item.LastUpdatedDate >= DateTime.UtcNow.AddDays(-7)
        };

        return response;
    }
}
