using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Variations.Commands;

public record ToggleExistsCommand(Guid Id, int ShopId) : IRequest<Unit>;

public class ToggleExistsCommandHandler(
    IInventoryItemRepository inventoryItemRepository)
    : IRequestHandler<ToggleExistsCommand, Unit>
{
    public async Task<Unit> Handle(ToggleExistsCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetInventoryItemByIdAndShopIdSpecification(request.Id, request.ShopId);

        var variation = await inventoryItemRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException("Variation not found");

        variation.Exists = !variation.Exists;

        await inventoryItemRepository.UpdateAsync(variation, cancellationToken);

        return Unit.Value;
    }
}