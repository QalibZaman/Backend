using Mapster;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries;

public record GetFilteredInventoryItemsQuery(GetFilteredInventoryItemsModel Model, int userShopId, int PageNumber, int PageSize) : IRequest<PaginatedResponse<InventoryItemResponse>>;

public class GetCategoriesQueryHandler(IInventoryItemRepository inventoryItemRepository, ICategoryRepository categoryRepository) : IRequestHandler<GetFilteredInventoryItemsQuery, PaginatedResponse<InventoryItemResponse>>
{
    public async Task<PaginatedResponse<InventoryItemResponse>> Handle(GetFilteredInventoryItemsQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<InventoryItemEntity>();

        if (!request.Model.ProductName.IsNullOrEmpty())
        {
            specificationBuilder.Add<GetInventoryItemsByPartialNameSpecification>(request.Model.ProductName);
        }

        if (!request.Model.ProductLocation.IsNullOrEmpty())
        {
            specificationBuilder.Add<GetInventoryItemsByPartialLocationSpecification>(request.Model.ProductLocation);
        }

        if (request.Model.CategoryId != 0)
        {
            var categories = await categoryRepository.GetCategorySubtreeAsync(request.Model.CategoryId, cancellationToken);
            var ids = categories.Select(c => c.Id).ToList();
            specificationBuilder.Add<GetInventoryItemsByCategoriesSpecification>(ids);
        }

        if (request.Model.MaxAvailableAmount != 0)
        {
            specificationBuilder.Add<GetInventoryItemsByMaxAvailableAmountSpecification>(request.Model.MaxAvailableAmount);
        }

        if (request.Model.MinAvailableAmount != 0)
        {
            specificationBuilder.Add<GetInventoryItemsByMinAvailableAmountSpecification>(request.Model.MinAvailableAmount);
        }

        if (request.Model.MaxReservedAmount != 0)
        {
            specificationBuilder.Add<GetInventoryItemsByMaxReservedAmountSpecification>(request.Model.MaxReservedAmount);
        }

        if (request.Model.MinReservedAmount != 0)
        {
            specificationBuilder.Add<GetInventoryItemsByMinReservedAmountSpecification>(request.Model.MinReservedAmount);
        }

        if (request.Model.MaxOnHandAmount != 0)
        {
            specificationBuilder.Add<GetInventoryItemsByMaxOnHandAmountSpecification>(request.Model.MaxOnHandAmount);
        }

        if (request.Model.MinOnHandAmount != 0)
        {
            specificationBuilder.Add<GetInventoryItemsByMinOnHandAmountSpecification>(request.Model.MinOnHandAmount);
        }

        if (request.Model.MaxPrice != 0)
        {
            specificationBuilder.Add<GetInventoryItemsByMaxPriceSpecification>(request.Model.MaxPrice);
        }

        if (request.Model.MinPrice != 0)
        {
            specificationBuilder.Add<GetInventoryItemsByMinPriceSpecification>(request.Model.MinPrice);
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

        var responseCollection = items.Adapt<List<InventoryItemResponse>>();

        var responseModel = new PaginatedResponse<InventoryItemResponse> { Items = responseCollection, TotalAmount = totalAmount };

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
