using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries;

public record GetInventoryItemDetailsQuery(Guid Id) : IRequest<IEnumerable<ProductDetailsResponse>>;
public class GetInventoryItemDetailsQueryHandler(
    IInventoryItemRepository inventoryItemRepository,
    IProductDetailsRepository productDetailsRepository)
    : IRequestHandler<GetInventoryItemDetailsQuery, IEnumerable<ProductDetailsResponse>>
{
    public async Task<IEnumerable<ProductDetailsResponse>> Handle(GetInventoryItemDetailsQuery request, CancellationToken cancellationToken)
    {
        var specification = new GetItemByIdSpecification(request.Id);

        var item = await inventoryItemRepository
           .GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException("There is no inventory item with such id.");

        var details = await productDetailsRepository.GetAllByPartitionKeyAsync(request.Id.ToString(), cancellationToken);

        var response = details.Adapt<IEnumerable<ProductDetailsResponse>>();

        return response;
    }
}
