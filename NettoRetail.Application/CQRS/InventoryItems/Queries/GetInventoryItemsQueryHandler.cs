using System.Linq.Expressions;
using Mapster;
using MediatR;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries;

public record GetInventoryItemsQuery(int ShopId, InventoryItemsSearchModel Model) : IRequest<PaginatedResponse<InventoryItemListResponse>>;

public class GetInventoryItemsQueryHandler(
    IInventoryItemRepository inventoryItemRepository,
    ICategoryRepository categoryRepository) : IRequestHandler<GetInventoryItemsQuery, PaginatedResponse<InventoryItemListResponse>>
{
    public async Task<PaginatedResponse<InventoryItemListResponse>> Handle(GetInventoryItemsQuery request, CancellationToken cancellationToken)
    {
        var model = request.Model;

        if (request.ShopId <= 0)
            throw new ArgumentException("ShopId is required and must be greater than zero.");

        if (!string.IsNullOrWhiteSpace(model.UpcOrName))
        {
            var specificationBuilder = new SpecificationBuilder<InventoryItemEntity>();
            specificationBuilder.Add<GetInventoryItemsByShopIdSpecification>(request.ShopId);

            if (long.TryParse(model.UpcOrName, out var upc))
            {
                specificationBuilder.Add<GetInventoryItemsByPartialUpcSpecification>(upc);
            }
            else
            {
                specificationBuilder.Add<GetInventoryItemsByPartialNameSpecification>(model.UpcOrName);
            }

            var skipItems = (model.PageNumber - 1) * model.PageSize;
            var specification = specificationBuilder.Skip(skipItems).Take(model.PageSize).Build();

            var (items, totalCount) = await inventoryItemRepository.GetAllPaginatedAsync(specification, cancellationToken);
            var responseItems = items.Adapt<List<InventoryItemListResponse>>();

            return new PaginatedResponse<InventoryItemListResponse>
            {
                Items = responseItems,
                TotalAmount = totalCount
            };
        }

        if (model.SortOption.HasValue)
        {
            var specification = new GetInventoryItemPriceSortingSpecification(request.ShopId, model.SortOption.Value);
            var sortedItems = await inventoryItemRepository.GetAllBySortedAsync(specification, cancellationToken);
            var responseItems = sortedItems.Adapt<List<InventoryItemListResponse>>();

            return new PaginatedResponse<InventoryItemListResponse>
            {
                Items = responseItems,
                TotalAmount = responseItems.Count
            };
        }

        var filterModel = new GetFilteredInventoryItemsModel
        {
            ProductName = model.ProductName,
            ProductLocation = model.ProductLocation,
            MinAvailableAmount = model.MinAvailableAmount,
            MinReservedAmount = model.MinReservedAmount,
            MinOnHandAmount = model.MinOnHandAmount,
            MinPrice = model.MinPrice,
            MaxPrice = model.MaxPrice,
            MaxOnHandAmount = model.MaxOnHandAmount,
            MaxAvailableAmount = model.MaxAvailableAmount,
            MaxReservedAmount = model.MaxReservedAmount,
            CategoryId = model.CategoryId ?? 0,
            Status = model.Status,
            BrandName = model.BrandName,
            SortingOptions = model.SortingOptions
        };

        var pageModel = new PaginationRequest
        {
            PageNumber = model.PageNumber,
            PageSize = model.PageSize
        };

        var specificationBuilderFiltered = new SpecificationBuilder<InventoryItemEntity>();
        var skip = (pageModel.PageNumber - 1) * pageModel.PageSize;

        specificationBuilderFiltered
            .Add<GetInventoryItemsByPartialNameSpecification>(filterModel.ProductName)
            .Add<GetInventoryItemsByPartialLocationSpecification>(filterModel.ProductLocation)
            .Add<GetInventoryItemsByMinAvailableAmountSpecification>(filterModel.MinAvailableAmount)
            .Add<GetInventoryItemsByMinReservedAmountSpecification>(filterModel.MinReservedAmount)
            .Add<GetInventoryItemsByMinOnHandAmountSpecification>(filterModel.MinOnHandAmount)
            .Add<GetInventoryItemsByMinPriceSpecification>(filterModel.MinPrice)
            .Add<GetInventoryItemsByShopIdSpecification>(request.ShopId);

        if (filterModel.MaxPrice != null)
            specificationBuilderFiltered.Add<GetInventoryItemsByMaxPriceSpecification>(filterModel.MaxPrice);
        if (filterModel.MaxOnHandAmount != null)
            specificationBuilderFiltered.Add<GetInventoryItemsByMaxOnHandAmountSpecification>(filterModel.MaxOnHandAmount);
        if (filterModel.MaxAvailableAmount != null)
            specificationBuilderFiltered.Add<GetInventoryItemsByMaxAvailableAmountSpecification>(filterModel.MaxAvailableAmount);
        if (filterModel.MaxReservedAmount != null)
            specificationBuilderFiltered.Add<GetInventoryItemsByMaxReservedAmountSpecification>(filterModel.MaxReservedAmount);

        if (filterModel.CategoryId != 0)
        {
            var categories = await categoryRepository.GetCategorySubtreeAsync(filterModel.CategoryId, cancellationToken);
            var ids = categories.Select(c => c.Id).ToList();
            specificationBuilderFiltered.Add<GetInventoryItemsByCategoriesSpecification>(ids);
        }

        if (!string.IsNullOrEmpty(filterModel.Status))
        {
            if (Enum.TryParse(filterModel.Status, true, out InventoryItemStatusEnum status))
            {
                specificationBuilderFiltered.Add<GetInventoryItemsByStatusSpecification>(status);
            }
        }

        if (!string.IsNullOrEmpty(filterModel.BrandName))
        {
            specificationBuilderFiltered.Add<GetInventoryItemsByBrandsPartialNameSpecification>(filterModel.BrandName);
        }

        if (filterModel.SortingOptions != null && filterModel.SortingOptions.Any())
        {
            foreach (var sortingOption in filterModel.SortingOptions)
            {
                specificationBuilderFiltered.AddOrderBy(GetSortingExpression(sortingOption.Field), sortingOption.IsDescending);
            }
        }

        var specificationFiltered = specificationBuilderFiltered.Skip(skip).Take(pageModel.PageSize).Build();

        var (filteredItems, totalFilteredCount) = await inventoryItemRepository.GetAllPaginatedAsync(specificationFiltered, cancellationToken);

        var responseFilteredItems = filteredItems.Adapt<List<InventoryItemListResponse>>();

        return new PaginatedResponse<InventoryItemListResponse>
        {
            Items = responseFilteredItems,
            TotalAmount = totalFilteredCount
        };
    }

    private static Expression<Func<InventoryItemEntity, object>> GetSortingExpression(string fieldName)
    {
        return fieldName.ToLower() switch
        {
            "productupc" => item => item.ProductUpc,
            "productlocation" => item => item.ProductLocation,
            "productname" => item => item.ProductName,
            "brandname" => item => item.Brand!.Name,
            "category" => item => item.Category!.Name,
            "availableamount" => item => item.AvailableAmount,
            "reservedamount" => item => item.ReservedAmount,
            "onhandamount" => item => item.OnHandAmount,
            "price" => item => item.Price,
            "lastupdateddate" => item => item.LastUpdatedDate,
            "status" => item => item.Status,
            _ => item => item.ProductName
        };
    }
}
