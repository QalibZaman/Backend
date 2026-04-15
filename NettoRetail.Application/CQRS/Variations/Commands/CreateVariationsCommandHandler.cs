using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Variation;
using NettoRetail.Application.Services.IInventoryService;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.DataAccess.Specifications.Inventory.Variations;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Variations.Commands;

public record CreateVariationsCommand(CreateVariationsRequest Model, int ShopId) : IRequest<Unit>;

public class CreateVariationsCommandHandler(
    IInventoryItemRepository inventoryItemRepository,
    IInventoryService inventoryService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateVariationsCommand, Unit>
{
    public async Task<Unit> Handle(CreateVariationsCommand command, CancellationToken cancellationToken)
    {
        var parentItemSpec = new GetInventoryItemByIdAndShopIdSpecification(command.Model.InventoryItemId, command.ShopId);
        var parentItem = await inventoryItemRepository.GetAsync(parentItemSpec, cancellationToken)
            ?? throw new BadRequestException("The parent variation does not exist");

        if (parentItem.IsVariation)
            throw new BadRequestException("A variation cannot be used as a parent item.");

        var existingSpec = new GetVariationsByParentIdSpecification(parentItem.Id, command.ShopId);
        var existingVariations = (await inventoryItemRepository.GetAllBySpecAsync(existingSpec, cancellationToken)).ToList();
        var existingNames = new HashSet<string>(existingVariations.Select(x => x.ProductName), StringComparer.OrdinalIgnoreCase);

        var incomingVariations = command.Model.Variations ?? [];
        var normalizedRequestNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var newVariations = new List<VariationModel>();

        foreach (var variation in incomingVariations)
        {
            if (variation == null)
                continue;

            if (string.IsNullOrWhiteSpace(variation.ProductName))
                continue;

            var normalizedName = variation.ProductName.Trim();

            if (!normalizedRequestNames.Add(normalizedName))
                continue;

            if (existingNames.Contains(normalizedName))
                continue;

            newVariations.Add(new VariationModel
            {
                ProductName = normalizedName,
                ProductUpc = variation.ProductUpc?.Trim() ?? string.Empty,
                IsActive = variation.IsActive
            });
        }

        var variations = inventoryService.BuildVariationInventoryItemEntities(newVariations, parentItem);

        if (!variations.Any() && parentItem.HasVariations)
            return Unit.Value;

        parentItem.HasVariations = existingVariations.Any() || variations.Any();

        using var transaction = unitOfWork.BeginTransaction();

        await inventoryItemRepository.UpdateAsync(parentItem, cancellationToken);

        if (variations.Any())
            await inventoryItemRepository.AddRangeAsync(variations, cancellationToken);

        transaction.Commit();

        return Unit.Value;
    }
}
