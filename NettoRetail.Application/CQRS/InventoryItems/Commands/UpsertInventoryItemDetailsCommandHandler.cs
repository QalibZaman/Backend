using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;

public record UpsertInventoryItemDetailsCommand(Guid InventoryItemId, int ShopId, UpsertInventoryItemDetailsModel Model) : IRequest;

public class UpsertInventoryItemDetailsCommandHandler(
    IInventoryItemRepository inventoryItemRepository,
    IProductDetailRepository productDetailRepository,
    IMapper mapper)
    : IRequestHandler<UpsertInventoryItemDetailsCommand>
{
    public async Task Handle(UpsertInventoryItemDetailsCommand request, CancellationToken cancellationToken)
    {
        var itemSpecification = new GetInventoryItemByIdAndShopIdSpecification(request.InventoryItemId, request.ShopId);

        var inventoryItemToUpdate = await inventoryItemRepository.GetAsync(itemSpecification, cancellationToken) 
            ?? throw new NotFoundException("There is no inventory item with such id.");

        var specification = new GetProductDetailsByItemIdSpecification(request.InventoryItemId);

        var inventoryItemDetails = await productDetailRepository.GetAllBySpecAsync(specification, cancellationToken);

        var idsList = inventoryItemDetails.Select(d => d.Id).ToHashSet();

        var detailsToUpdate = new List<ProductDetailEntity>();
        var detailsToInsert = new List<ProductDetailEntity>();

        foreach (var detail in request.Model.Details)
        {
            var entity = mapper.Map<ProductDetailEntity>(detail);
            entity.InventoryItemId = request.InventoryItemId;

            if (detail.Id.HasValue && idsList.Contains(detail.Id.Value))
            {
                detailsToUpdate.Add(entity);
            }
            else
            {
                detailsToInsert.Add(entity);
            }
        }

        if (detailsToUpdate.Any())
            await productDetailRepository.UpdateManyAsync(detailsToUpdate, cancellationToken);

        if (detailsToInsert.Any())
            await productDetailRepository.AddRangeAsync(detailsToInsert, cancellationToken);
    }
}
