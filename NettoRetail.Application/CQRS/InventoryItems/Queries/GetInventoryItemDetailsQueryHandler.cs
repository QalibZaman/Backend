using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries;

public record GetInventoryItemDetailsQuery(Guid Id) : IRequest<ProductDetailsResponse>;
public class GetInventoryItemDetailsQueryHandler(
    IInventoryItemRepository inventoryItemRepository,
    IProductDetailsRepository productDetailsRepository)
    : IRequestHandler<GetInventoryItemDetailsQuery, ProductDetailsResponse>
{
    public async Task<ProductDetailsResponse> Handle(GetInventoryItemDetailsQuery request, CancellationToken cancellationToken)
    {
        var specification = new GetItemByIdSpecification(request.Id);

        var item = await inventoryItemRepository
           .GetAsync(specification, cancellationToken);

        if (item == null)
        {
           throw new NotFoundException("There is no inventory item with such id.");
        }

        var details = await productDetailsRepository.GetAllByPartitionKeyAsync(request.Id.ToString(), cancellationToken);

        var response = details.Adapt<ProductDetailsResponse>();

        return response;
    }
}
