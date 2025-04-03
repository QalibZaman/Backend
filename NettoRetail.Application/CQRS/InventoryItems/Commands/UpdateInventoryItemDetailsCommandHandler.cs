using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record UpdateInventoryItemDetailsCommand(Guid InventoryItemId, int ShopId, UpdateInventoryItemDetailsModel Model) : IRequest;

public class UpdateInventoryItemDetailsCommandHandler(IInventoryItemRepository inventoryItemRepository, IProductDetailsRepository productDetailsRepository)
    : IRequestHandler<UpdateInventoryItemDetailsCommand>
{
    public async Task Handle(UpdateInventoryItemDetailsCommand request, CancellationToken cancellationToken)
    {
        var detailsToUpsert = new List<ProductDetailsEntity>();

        var itemSpecification = new GetInventoryItemByIdSpecification(request.InventoryItemId);
        var inventoryItemToUpdate = await inventoryItemRepository.GetAsync(itemSpecification, cancellationToken) ?? throw new NotFoundException("There is no inventory item with such id.");

        if (inventoryItemToUpdate.ShopId != request.ShopId)
            throw new MyUnauthorizedException("This item is inaccessible");

        var inventoryItemDetailsToUpdate = await productDetailsRepository.GetAllByPartitionKeyAsync(request.InventoryItemId.ToString(), cancellationToken);

        foreach (var detail in request.Model.Details)
        {
            var detailToUpsert = inventoryItemDetailsToUpdate.FirstOrDefault(d => d.Id == detail.Id.ToString());

            if (detailToUpsert == null)
            {
                detailToUpsert = detail.Adapt<ProductDetailsEntity>();
                detailToUpsert.Id = Guid.NewGuid().ToString();
            }
            else
            {
                detail.Adapt(detailToUpsert);
            }

            detailsToUpsert.Add(detailToUpsert);
        }

        await productDetailsRepository.ExecuteUpdatingTransactionalBatchAsync(request.InventoryItemId.ToString(), detailsToUpsert, request.Model.DetailsToDelete, cancellationToken);
    }
}