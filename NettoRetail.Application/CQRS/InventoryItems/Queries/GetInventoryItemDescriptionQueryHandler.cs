using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Queries
{
    public record GetInventoryItemDescriptionQuery(Guid InventoryItemId) : IRequest<string>;
    public class GetInventoryItemDescriptionQueryHandler(IInventoryItemRepository inventoryItemRepository) : IRequestHandler<GetInventoryItemDescriptionQuery, string>
    {
        public async Task<string> Handle(GetInventoryItemDescriptionQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetInventoryItemByIdSpecification(request.InventoryItemId);

            var item = await inventoryItemRepository.GetAsync(spec, cancellationToken)
                ?? throw new NotFoundException("Inventory item not found");

            return item.Description ?? string.Empty;
        }
    }
}
