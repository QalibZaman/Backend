using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record UpdateInventoryItemDescriptionCommand(Guid InventoryItemId, string Description) : IRequest<Unit>;
public class UpdateInventoryItemDescriptionCommandHandler(IInventoryItemRepository inventoryItemRepository) : IRequestHandler<UpdateInventoryItemDescriptionCommand, Unit>
{
    public async Task<Unit> Handle(UpdateInventoryItemDescriptionCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetInventoryItemByIdSpecification(request.InventoryItemId);

        var item = await inventoryItemRepository.GetAsync(spec,cancellationToken)
            ?? throw new NotFoundException("Inventory item not found");

        item.Description = request.Description;

        await inventoryItemRepository.UpdateAsync(item, cancellationToken);
        return Unit.Value;
    }
}
