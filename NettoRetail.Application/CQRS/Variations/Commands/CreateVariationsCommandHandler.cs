using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Variation;
using NettoRetail.Application.Services.IInventoryService;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Variations.Commands
{
    public record CreateVariationsCommand(CreateVariationsRequest Model, int ShopId) : IRequest<Unit>;
    public class CreateVariationsCommandHandler(IInventoryItemRepository inventoryItemRepository,
        IInventoryService inventoryService,
        IUnitOfWork unitOfWork)
        : IRequestHandler<CreateVariationsCommand, Unit>
    {
        public async Task<Unit> Handle(CreateVariationsCommand command, CancellationToken cancellationToken)
        {
            var parentItemSpec = new GetInventoryItemByIdAndShopIdSpecification(command.Model.InventoryItemId, command.ShopId);
            var parentItem = await inventoryItemRepository.GetAsync(parentItemSpec, cancellationToken)
                ?? throw new BadRequestException("The parent variation does not exist");

            var variations = inventoryService.BuildVariationInventoryItemEntities(command.Model.Variations, parentItem);

            parentItem.HasVariations = true;

            using var transaction = unitOfWork.BeginTransaction();

            await inventoryItemRepository.UpdateAsync(parentItem, cancellationToken);
            await inventoryItemRepository.AddRangeAsync(variations, cancellationToken);

            transaction.Commit();

            return Unit.Value;
        }
    }
}
