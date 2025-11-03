using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries;

public record GetInventoryItemDetailsQuery(Guid Id) : IRequest<List<ProductDetailsResponse>>;
public class GetInventoryItemDetailsQueryHandler(
    IInventoryItemRepository inventoryItemRepository,
    IProductDetailRepository productDetailRepository)
    : IRequestHandler<GetInventoryItemDetailsQuery, List<ProductDetailsResponse>>
{
    public async Task<List<ProductDetailsResponse>> Handle(GetInventoryItemDetailsQuery request, CancellationToken cancellationToken)
    {
        var itemSpecification = new GetItemByIdSpecification(request.Id);

        var itemExists = await inventoryItemRepository.Exists(itemSpecification, cancellationToken);

        if (!itemExists)
        {
            throw new NotFoundException("There are no items with such id");
        }

        var detailsSpecification = new GetProductDetailsByItemIdSpecification(request.Id);

        var productDetails = await productDetailRepository.GetAllBySpecAsync(detailsSpecification, cancellationToken);

        var response = productDetails.Adapt<List<ProductDetailsResponse>>();

        return response;
    }
}
