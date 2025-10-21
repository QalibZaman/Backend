using MediatR;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Shop;
using NettoRetail.Application.Services.IInventoryService;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Shops.Queries;

public record GetShopFilteredCatalogQuery(GetFilteredCatalogModel FilteredCatalogModel, int ShopId, PaginationRequest PageModel) : IRequest<PaginatedResponse<InventoryItemCardResponse>>;

public class GetShopFilteredCatalogHandler(IInventoryItemRepository itemsRepository, IInventoryService inventoryService) : IRequestHandler<GetShopFilteredCatalogQuery, PaginatedResponse<InventoryItemCardResponse>>
{
    public async Task<PaginatedResponse<InventoryItemCardResponse>> Handle(GetShopFilteredCatalogQuery request, CancellationToken cancellationToken)
    {
        var specification = BuildInventoryItemSpecification(request);

        var (items, totalAmount) = await itemsRepository.GetAllPaginatedAsync(specification, cancellationToken);

        var responseItems = items.Select(i =>
        {
            var imgs = i.Images.OrderBy(im => im.Order).ToList();

            return new InventoryItemCardResponse
            {
                Id = i.Id,
                ProductName = i.ProductName,
                CategoryName = i.Category.Name,
                BrandName = i.Brand?.Name ?? string.Empty,
                Status = i.Status.ToString(),
                MediaUrls = imgs.Select(im => im.BlobUrl),
                ThumbnailUrl = imgs.FirstOrDefault()?.BlobUrl ?? ImagePlaceholdersConstants.InventoryItem,
                Price = i.Price,
                PromotionPrice = i.PromotionPrice,
                AvailableAmount = i.AvailableAmount,
                IsOnSale = inventoryService.IsItemOnSale(i),
                IsOutOfStock = inventoryService.IsItemOutOfStock(i),
                IsNew = inventoryService.IsItemNew(i),
                LastUpdatedDate = i.LastUpdatedDate
            };
        }).ToList();

        var response = new PaginatedResponse<InventoryItemCardResponse>
        {
            Items = responseItems,
            TotalAmount = totalAmount
        };
        return response;
    }

    private Specification<InventoryItemEntity> BuildInventoryItemSpecification(GetShopFilteredCatalogQuery request)
    {
        var specBuilder = new SpecificationBuilder<InventoryItemEntity>();

        specBuilder.Add<GetInventoryItemsByShopIdSpecification>(request.ShopId);

        if (request.FilteredCatalogModel.BrandsNames != null)
        {
            specBuilder.Add<GetInventoryItemsByListOfBrandsPartialNameSpecification>(request.FilteredCatalogModel.BrandsNames);
        }

        if (request.FilteredCatalogModel.IsPromoted != null)
        {
            specBuilder.Add<GetInventoryItemsByPromotionSpecification>(request.FilteredCatalogModel.IsPromoted);
        }

        if (request.FilteredCatalogModel.MinPrice != null)
        {
            specBuilder.Add<GetInventoryItemsByMinPriceSpecification>(request.FilteredCatalogModel.MinPrice);
        }

        if (request.FilteredCatalogModel.MaxPrice != null)
        {
            specBuilder.Add<GetInventoryItemsByMaxPriceSpecification>(request.FilteredCatalogModel.MaxPrice);
        }

        if (request.FilteredCatalogModel.ProductName != null)
        {
            specBuilder.Add<GetInventoryItemsByPartialNameSpecification>(request.FilteredCatalogModel.ProductName);
        }

        if (!request.FilteredCatalogModel.ItemStatus.IsNullOrEmpty())
        {
            if (Enum.TryParse(request.FilteredCatalogModel.ItemStatus, true, out InventoryItemStatusEnum status))
            {
                specBuilder.Add<GetInventoryItemsByStatusSpecification>(status);
            }
        }

        if (request.FilteredCatalogModel.SortingBy != null)
        {
            switch (request.FilteredCatalogModel.SortingBy)
            {
                case ShopCatalogSortingEnum.PriceLowToHigh:
                    specBuilder.AddOrderBy(x => x.IsPromoted ? x.PromotionPrice! : x.Price);
                    break;

                case ShopCatalogSortingEnum.PriceHighToLow:
                    specBuilder.AddOrderBy(x => x.IsPromoted ? x.PromotionPrice! : x.Price, true);
                    break;

                case ShopCatalogSortingEnum.Newest:
                    specBuilder.AddOrderBy(x => x.LastUpdatedDate, true);
                    break;
            }
        }

        specBuilder.AddInclude(x => x.Brand!)
            .AddInclude(x => x.Category)
            .AddInclude(x => x.Images);

        var skip = (request.PageModel.PageNumber - 1) * request.PageModel.PageSize;

        specBuilder.Skip(skip).Take(request.PageModel.PageSize);

        return specBuilder.Build();
    }
}
