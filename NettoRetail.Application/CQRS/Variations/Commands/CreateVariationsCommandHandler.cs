using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Variation;
using NettoRetail.Application.Services.IInventoryService;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.DataAccess.Specifications.Inventory.Variations;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Variations.Commands;

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

        ValidateParent(parentItem);
        await ValidateUniqueUpcsAsync(command.Model, cancellationToken);

        var variations = inventoryService.BuildVariationInventoryItemEntities(command.Model.Variations, parentItem);

        parentItem.HasVariations = true;
        parentItem.LastUpdatedDate = DateTime.UtcNow;

        using var transaction = unitOfWork.BeginTransaction();

        await inventoryItemRepository.UpdateAsync(parentItem, cancellationToken);
        await inventoryItemRepository.AddRangeAsync(variations, cancellationToken);

        transaction.Commit();

        return Unit.Value;
    }

    private static void ValidateParent(InventoryItemEntity parentItem)
    {
        if (parentItem.IsVariation)
            throw new BadRequestException("A variation cannot be used as a parent product.");

        if (parentItem.HasVariations)
            throw new BadRequestException("Variations have already been created for this product.");
    }

    private async Task ValidateUniqueUpcsAsync(CreateVariationsRequest request, CancellationToken cancellationToken)
    {
        foreach (var variation in request.Variations)
        {
            var spec = new GetProductByUpcSpecification(variation.ProductUpc);

            if (await inventoryItemRepository.ExistsAsync(spec, cancellationToken))
                throw new ConflictException($"This product UPC is already registered: {variation.ProductUpc}");
        }
    }
}
