using Mapster;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Shop;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Shops.Queries;

public record GetShopFilteredCatalogQuery(GetFilteredCatalogModel FilteredCatalogModel, int ShopId, PaginationRequest PageModel) : IRequest<PaginatedResponse<InventoryItemListResponse>>;

public class GetShopFilteredCatalogHandler(IInventoryItemRepository itemsRepository) : IRequestHandler<GetShopFilteredCatalogQuery, PaginatedResponse<InventoryItemListResponse>>
{
    public async Task<PaginatedResponse<InventoryItemListResponse>> Handle(GetShopFilteredCatalogQuery request, CancellationToken cancellationToken)
    {
        var specification = BuildInventoryItemSpecification(request);

        var (items, totalAmount) = await itemsRepository.GetAllPaginatedAsync(specification, cancellationToken);

        var responseItems = items.Adapt<List<InventoryItemListResponse>>();

        var response = new PaginatedResponse<InventoryItemListResponse>
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

        specBuilder.AddInclude(x => x.Brand!)
            .AddInclude(x => x.Category);

        var skip = (request.PageModel.PageNumber - 1) * request.PageModel.PageSize;

        specBuilder.Skip(skip).Take(request.PageModel.PageSize);

        return specBuilder.Build();
    }
}
