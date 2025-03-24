using System.Linq.Expressions;
using Mapster;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries;

public record GetFilteredInventoryItemsQuery(GetFilteredInventoryItemsModel Model, int ShopId, int PageNumber, int PageSize) : IRequest<PaginatedResponse<InventoryItemListResponse>>;

public class GetCategoriesQueryHandler(IInventoryItemRepository inventoryItemRepository, ICategoryRepository categoryRepository) : IRequestHandler<GetFilteredInventoryItemsQuery, PaginatedResponse<InventoryItemListResponse>>
{
    public async Task<PaginatedResponse<InventoryItemListResponse>> Handle(GetFilteredInventoryItemsQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<InventoryItemEntity>();

        specificationBuilder.Add<GetInventoryItemsByPartialNameSpecification>(request.Model.ProductName)
            .Add<GetInventoryItemsByPartialLocationSpecification>(request.Model.ProductLocation)
            .Add<GetInventoryItemsByMaxAvailableAmountSpecification>(request.Model.MaxAvailableAmount)
            .Add<GetInventoryItemsByMinAvailableAmountSpecification>(request.Model.MinAvailableAmount)
            .Add<GetInventoryItemsByMaxReservedAmountSpecification>(request.Model.MaxReservedAmount)
            .Add<GetInventoryItemsByMinReservedAmountSpecification>(request.Model.MinReservedAmount)
            .Add<GetInventoryItemsByMaxOnHandAmountSpecification>(request.Model.MaxOnHandAmount)
            .Add<GetInventoryItemsByMinOnHandAmountSpecification>(request.Model.MinOnHandAmount)
            .Add<GetInventoryItemsByMaxPriceSpecification>(request.Model.MaxPrice)
            .Add<GetInventoryItemsByMinPriceSpecification>(request.Model.MinPrice)
            .Add<GetInventoryItemsByShopIdSpecification>(request.ShopId);

        specificationBuilder.AddInclude(x => x.Category);

        if (request.Model.CategoryId != 0)
        {
            var categories = await categoryRepository.GetCategorySubtreeAsync(request.Model.CategoryId, cancellationToken);
            var ids = categories.Select(c => c.Id).ToList();
            specificationBuilder.Add<GetInventoryItemsByCategoriesSpecification>(ids);
        }

        if (!request.Model.Status.IsNullOrEmpty())
        {
            if (Enum.TryParse(request.Model.Status, true, out InventoryItemStatusEnum status))
            {
                specificationBuilder.Add<GetInventoryItemsByStatusSpecification>(status);
            }
        }

        if (request.Model.SortingOptions != null && request.Model.SortingOptions.Any())
        {
            foreach (var sortingOption in request.Model.SortingOptions)
            {
                specificationBuilder.AddOrderBy(GetSortingExpression(sortingOption.Field), sortingOption.IsDescending);
            }
        }

        var specification = specificationBuilder.Skip(request.PageNumber * request.PageSize).Take(request.PageSize).Build();

        var items = await inventoryItemRepository.GetPaginatedAsync(specification, cancellationToken);

        var totalAmount = await inventoryItemRepository.GetFilteredAmountAsync(specification, cancellationToken);

        var responseCollection = items.Adapt<List<InventoryItemListResponse>>();

        var responseModel = new PaginatedResponse<InventoryItemListResponse> { Items = responseCollection, TotalAmount = totalAmount };

        return responseModel;
    }

    private static Expression<Func<InventoryItemEntity, object>> GetSortingExpression(string fieldName)
    {
        return fieldName.ToLower() switch
        {
            "productname" => item => item.ProductName,
            "price" => item => item.Price,
            "availableamount" => item => item.AvailableAmount,
            "reservedamount" => item => item.ReservedAmount,
            "onhandamount" => item => item.OnHandAmount,
            _ => item => item.ProductName
        };
    }
}
