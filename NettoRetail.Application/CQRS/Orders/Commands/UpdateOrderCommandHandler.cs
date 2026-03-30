using MediatR;
using Microsoft.EntityFrameworkCore;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Order;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Orders.Commands;

public record UpdateOrderCommand(UpdateOrderModel Model, int ShopId, Guid UserId, int OrderId) : IRequest;

public class UpdateOrderCommandHandler(
        IOrderRepository orderRepository,
        IInventoryItemRepository inventoryItemRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateOrderCommand>
{
    public async Task Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<OrderEntity>();
        var specification = specificationBuilder.Add<GetOrdersByIdSpecification>(request.OrderId)
            .AddInclude(x => x.OrderItems)
            .Build();

        var order = await orderRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException(HandlerConstants.Order.OrderNotFoundMessage);

        if (order.OrderStatus == OrderStatusEnum.InProgress)
            throw new BadRequestException(HandlerConstants.Order.OrderCannotBeEditedMessage);

        var inventoryItemIds = request.Model.ItemsToUpsert.Keys
            .Concat(request.Model.ItemsToDelete ?? Enumerable.Empty<Guid>())
            .Distinct()
            .ToList();

        var getItemsByIdsAndShopSpecification = new GetInventoryItemsByIdsAndShopSpecification(inventoryItemIds, request.ShopId);
        var inventoryItems = await inventoryItemRepository.GetManyBySpecAsync(getItemsByIdsAndShopSpecification, cancellationToken)
            ?? throw new NotFoundException(HandlerConstants.Inventory.InventoryItemsNotFoundMessage);

        var inventoryById = inventoryItems.ToDictionary(x => x.Id);
        var itemsToUpdate = new HashSet<InventoryItemEntity>();

        order.DeliveryType = (OrderDeliveryTypeEnum)request.Model.DeliveryType!;

        using var transaction = unitOfWork.BeginTransaction();
        if (request.Model.ItemsToDelete != null)
        {
            ProcessDeletion(order, request.Model.ItemsToDelete, inventoryById, itemsToUpdate);
        }

        ProcessUpserts(order, request.Model.ItemsToUpsert, inventoryById, itemsToUpdate);

        if (itemsToUpdate.Count > 0)
        {
            await inventoryItemRepository.UpdateManyAsync(itemsToUpdate.ToList(), cancellationToken);
        }

        await orderRepository.UpdateAsync(order, cancellationToken);
        transaction.Commit();
    }

    private void ProcessUpserts(
        OrderEntity order,
        IDictionary<Guid, int> itemsToUpsert,
        Dictionary<Guid, InventoryItemEntity> inventory,
        HashSet<InventoryItemEntity> itemsToUpdate)
    {
        foreach (var (itemId, quantity) in itemsToUpsert)
        {
            if (!inventory.TryGetValue(itemId, out var stockItem))
            {
                throw new NotFoundException(string.Format(HandlerConstants.Inventory.InventoryItemNotFoundMessage, itemId));
            }

            var existingItem = order.OrderItems.FirstOrDefault(oi => oi.ItemId == itemId);
            int currentQuantity = existingItem?.Quantity ?? 0;
            int delta = quantity - currentQuantity;

            if (delta > 0 && stockItem.AvailableAmount < delta)
            {
                throw new BadRequestException(string.Format(HandlerConstants.Order.InsufficientStockItem, itemId));
            }

            stockItem.AvailableAmount -= delta;
            stockItem.ReservedAmount += delta;
            ValidateInventoryAmounts(stockItem);

            itemsToUpdate.Add(stockItem);

            if (existingItem == null)
            {
                if (quantity > 0)
                {
                    order.OrderItems.Add(new OrderItemEntity { ItemId = itemId, Quantity = quantity, OrderId = order.Id });
                }
            }
            else if (existingItem != null)
            {
                if (quantity == 0)
                {
                    order.OrderItems.Remove(existingItem);
                }
                else
                {
                    existingItem.Quantity = quantity;
                }
            }
        }
    }

    private void ProcessDeletion(
        OrderEntity order,
        IEnumerable<Guid> itemsToDelete,
        Dictionary<Guid, InventoryItemEntity> inventory,
        HashSet<InventoryItemEntity> itemsToUpdate)
    {
        foreach (var itemId in itemsToDelete)
        {
            var itemToRemove = order.OrderItems.FirstOrDefault(x => x.ItemId == itemId)
                ?? throw new NotFoundException(string.Format(HandlerConstants.Inventory.InventoryItemNotFoundMessage, itemId));

            if (inventory.TryGetValue(itemId, out var stockItem))
            {
                stockItem.AvailableAmount += itemToRemove.Quantity;
                stockItem.ReservedAmount -= itemToRemove.Quantity;
                ValidateInventoryAmounts(stockItem);
                itemsToUpdate.Add(stockItem);
            }

            order.OrderItems.Remove(itemToRemove);
        }
    }

    private void ValidateInventoryAmounts(InventoryItemEntity item)
    {
        if (item.AvailableAmount < 0)
            throw new BadRequestException(string.Format(HandlerConstants.Inventory.AvailableAmountNotEnoughMessage, item.Id));

        if (item.ReservedAmount < 0)
            throw new BadRequestException(string.Format(HandlerConstants.Inventory.ReservedAmountNotEnoughMessage, item.Id));

        if (item.AvailableAmount + item.ReservedAmount > item.OnHandAmount)
            throw new BadRequestException(string.Format(HandlerConstants.Inventory.ReservedAvailableCannotExceedOnHand, item.Id));
    }
}
