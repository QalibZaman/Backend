using System.Linq.Expressions;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries;

public record GetFilteredInventoryItemsQuery(GetFilteredInventoryItemsModel Model, int ShopId, PaginationRequest PageModel) : IRequest<PaginatedResponse<InventoryItemListResponse>>;

public class GetFilteredInventoryItemsQueryHandler(IInventoryItemRepository inventoryItemRepository, ICategoryRepository categoryRepository) : IRequestHandler<GetFilteredInventoryItemsQuery, PaginatedResponse<InventoryItemListResponse>>
{
    public async Task<PaginatedResponse<InventoryItemListResponse>> Handle(GetFilteredInventoryItemsQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<InventoryItemEntity>();
        var skip = (request.PageModel.PageNumber - 1) * request.PageModel.PageSize;

        specificationBuilder.Add<GetInventoryItemsByPartialNameSpecification>(request.Model.ProductName)
            .Add<GetInventoryItemsByPartialLocationSpecification>(request.Model.ProductLocation)
            .Add<GetInventoryItemsByMinAvailableAmountSpecification>(request.Model.MinAvailableAmount)
            .Add<GetInventoryItemsByMinReservedAmountSpecification>(request.Model.MinReservedAmount)
            .Add<GetInventoryItemsByMinOnHandAmountSpecification>(request.Model.MinOnHandAmount)
            .Add<GetInventoryItemsByMinPriceSpecification>(request.Model.MinPrice)
            .Add<GetInventoryItemsByShopIdSpecification>(request.ShopId);

        if (request.Model.MaxPrice != null)
        {
            specificationBuilder.Add<GetInventoryItemsByMaxPriceSpecification>(request.Model.MaxPrice);
        }

        if (request.Model.MaxOnHandAmount != null)
        {
            specificationBuilder.Add<GetInventoryItemsByMaxOnHandAmountSpecification>(request.Model.MaxOnHandAmount);
        }

        if (request.Model.MaxAvailableAmount != null)
        {
            specificationBuilder.Add<GetInventoryItemsByMaxAvailableAmountSpecification>(request.Model.MaxAvailableAmount);
        }

        if (request.Model.MaxReservedAmount != null)
        {
            specificationBuilder.Add<GetInventoryItemsByMaxReservedAmountSpecification>(request.Model.MaxReservedAmount);
        }

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

        specificationBuilder.AddInclude(x => x.Category);

        var specification = specificationBuilder.Skip(skip).Take(request.PageModel.PageSize).Build();

        var (items, totalAmount) = await inventoryItemRepository.GetAllPaginatedAsync(specification, cancellationToken);

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
