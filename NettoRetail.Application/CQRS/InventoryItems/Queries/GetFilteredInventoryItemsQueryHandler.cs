using Mapster;
using MediatR;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries;

public record GetFilteredInventoryItemsQuery(GetFilteredInventoryItemsModel Model, int userShopId, int PageNumber, int PageSize) : IRequest<PaginatedResponse<InventoryItemResponse>>;

public class GetCategoriesQueryHandler(IInventoryItemRepository inventoryItemRepository) : IRequestHandler<GetFilteredInventoryItemsQuery, PaginatedResponse<InventoryItemResponse>>
{
    public async Task<PaginatedResponse<InventoryItemResponse>> Handle(GetFilteredInventoryItemsQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<InventoryItemEntity>();

        if (request.Model.ProductName != null)
        {
            var nameSpecification = new GetInventoryItemsByPartialNameSpecification(request.Model.ProductName);
            specificationBuilder.Add<GetInventoryItemsByPartialNameSpecification>(request.Model.ProductName);
        }

        var getSpecification = specificationBuilder.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).Build();

        var items = await inventoryItemRepository.GetPaginatedAsync(getSpecification, cancellationToken);

        var totalAmount = await inventoryItemRepository.GetFilteredAmountAsync(getSpecification, cancellationToken);

        var responseCollection = items.Adapt<List<InventoryItemResponse>>();

        var responseModel = new PaginatedResponse<InventoryItemResponse> { Items = responseCollection, TotalAmount = totalAmount };

        return responseModel;
    }
}
