using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record UpdateInventoryItemDetailsCommand(Guid InventoryItemId, UpdateInventoryItemDetailsModel Model) : IRequest;

public class UpdateInventoryItemDetailsCommandHandler(IInventoryItemRepository inventoryItemRepository, IProductDetailsRepository productDetailsRepository)
    : IRequestHandler<UpdateInventoryItemDetailsCommand>
{
    public async Task Handle(UpdateInventoryItemDetailsCommand request, CancellationToken cancellationToken)
    {
        var itemSpecification = new GetInventoryItemByIdSpecification(request.InventoryItemId);
        var inventoryItemToUpdate = await inventoryItemRepository.GetAsync(itemSpecification, cancellationToken) ?? throw new NotFoundException("There is no inventory item with such id.");

        var inventoryItemDetailsToUpdate = await productDetailsRepository.GetAllByPartitionKeyAsync(request.InventoryItemId.ToString(), cancellationToken);

        foreach (var detail in request.Model.Details)
        {
            var detailToUpdate = inventoryItemDetailsToUpdate.FirstOrDefault(d => d.Id == detail.Id) ?? throw new NotFoundException($"Could not find a detail with id {detail.Id}");
            detail.Adapt(detailToUpdate);
            await productDetailsRepository.UpsertAsync(detailToUpdate, cancellationToken);
        }

        foreach (var detailId in request.Model.DetailsToDelete)
        {
            await productDetailsRepository.DeleteAsync(detailId.ToString(), request.InventoryItemId.ToString(), cancellationToken);
        }
    }
}