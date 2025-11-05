using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record UpdateInventoryItemDetailsCommand(Guid InventoryItemId, int ShopId, UpdateInventoryItemDetailsModel Model) : IRequest;

public class UpdateInventoryItemDetailsCommandHandler(IInventoryItemRepository inventoryItemRepository, IProductDetailRepository productDetailRepository)
    : IRequestHandler<UpdateInventoryItemDetailsCommand>
{
    public async Task Handle(UpdateInventoryItemDetailsCommand request, CancellationToken cancellationToken)
    {
        var detailsToUpsert = new List<ProductDetailEntity>();

        var itemSpecification = new GetInventoryItemByIdAndShopIdSpecification(request.InventoryItemId, request.ShopId);
        var inventoryItemToUpdate = await inventoryItemRepository.GetAsync(itemSpecification, cancellationToken) ?? throw new NotFoundException("There is no inventory item with such id.");

        var specification = new GetProductDetailsByItemIdSpecification(request.InventoryItemId);

        var inventoryItemDetailsToUpdate = await productDetailRepository.GetAllBySpecAsync(specification, cancellationToken);


        foreach (var detail in request.Model.Details)
        {
            var detailToUpsert = inventoryItemDetailsToUpdate.FirstOrDefault(d => d.Id.ToString() == detail.Id.ToString());

            if (detailToUpsert == null)
            {
                detailToUpsert = detail.Adapt<ProductDetailEntity>();
                detailToUpsert.Id = Guid.NewGuid();
            }
            else
            {
                detail.Adapt(detailToUpsert);
            }

            detailToUpsert.InventoryItemId = request.InventoryItemId;
            detailsToUpsert.Add(detailToUpsert);
        }

        await productDetailRepository.ExecuteUpdatingTransactionalBatchAsync(request.InventoryItemId.ToString(), detailsToUpsert, request.Model.DetailsToDelete, cancellationToken);
    }
}