using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Order;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Orders.Commands;

public record UpdateOrderCommand(UpdateOrderModel Model, int ShopId, Guid UserId, int OrderId) : IRequest;

public class UpdateOrderCommandHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IInventoryItemRepository inventoryItemRepository,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateOrderCommand>
{
    public async Task Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<OrderEntity>();
        specificationBuilder.Add<GetOrdersByIdSpecification>(request.OrderId);
        specificationBuilder.AddInclude(x => x.OrderItems);

        var order = await orderRepository.GetAsync(specificationBuilder.Build(), cancellationToken)
            ?? throw new NotFoundException("There is no such order.");

        if (order.OrderStatus == OrderStatusEnum.InProgress)
            throw new BadRequestException("This order cannot be edited while in 'In Progress' status.");

        var customerSpecification = new GetCustomerByIdSpecification(request.Model.CustomerId);
        var customerExists = await customerRepository.Exists(customerSpecification, cancellationToken);
        if (!customerExists)
        {
            throw new NotFoundException("There is no such customer.");
        }

        var inventoryItemIds = request.Model.ItemsToUpsert.Keys
            .Concat(request.Model.ItemsToDelete)
            .Distinct()
            .ToList();

        var getItemsByIdsSpecification = new GetItemsByIdsSpecification(inventoryItemIds);
        var inventoryItems = await inventoryItemRepository.GetManyBySpecAsync(getItemsByIdsSpecification, cancellationToken)
            ?? throw new NotFoundException("There are no such items.");

        var inventoryById = inventoryItems.ToDictionary(x => x.Id);
        var changedItems = new HashSet<InventoryItemEntity>();

        var transaction = unitOfWork.BeginTransaction();

        try
        {
            request.Model.Adapt(order);

            foreach (var itemToUpsert in request.Model.ItemsToUpsert)
            {
                var itemId = itemToUpsert.Key;
                var newQty = itemToUpsert.Value;

                if (!inventoryById.TryGetValue(itemId, out var item))
                    throw new NotFoundException($"There is no such inventory item (Id: {itemId}).");

                var existingOrderItem = order.OrderItems.FirstOrDefault(x => x.ItemId == itemId);
                var oldQty = existingOrderItem?.Quantity ?? 0;
                var delta = newQty - oldQty;

                if (existingOrderItem is null)
                {
                    order.OrderItems.Add(new OrderItemEntity
                    {
                        ItemId = itemId,
                        Quantity = newQty,
                        OrderId = request.OrderId
                    });
                }
                else
                {
                    existingOrderItem.Quantity = newQty;
                }

                item.AvailableAmount -= delta;
                item.ReservedAmount += delta;

                ValidateInventoryAmounts(item);
                changedItems.Add(item);
            }

            foreach (var itemIdToDelete in request.Model.ItemsToDelete.Distinct())
            {
                if (!inventoryById.TryGetValue(itemIdToDelete, out var item))
                    throw new NotFoundException($"There is no such inventory item (Id: {itemIdToDelete}).");

                var itemToRemove = order.OrderItems.FirstOrDefault(x => x.ItemId == itemIdToDelete)
                    ?? throw new NotFoundException("One of the inventory items to delete is inaccessible.");

                item.AvailableAmount += itemToRemove.Quantity;
                item.ReservedAmount -= itemToRemove.Quantity;

                ValidateInventoryAmounts(item);
                changedItems.Add(item);

                order.OrderItems.Remove(itemToRemove);
            }

            if (changedItems.Count > 0)
                await inventoryItemRepository.UpdateManyAsync(changedItems.ToList(), cancellationToken);

            if (order.OrderItems.Any())
                await orderRepository.UpdateAsync(order, cancellationToken);
            else
                await orderRepository.DeleteAsync(order, cancellationToken);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static void ValidateInventoryAmounts(InventoryItemEntity item)
    {
        if (item.AvailableAmount < 0)
            throw new BadRequestException($"Available amount must be a non-negative integer (item {item.Id}).");

        if (item.ReservedAmount < 0)
            throw new BadRequestException($"Reserved amount must be a non-negative integer (item {item.Id}).");

        if (item.ReservedAmount > item.AvailableAmount)
            throw new BadRequestException($"Reserved amount cannot exceed available amount (item {item.Id}).");

        if (item.ReservedAmount + item.AvailableAmount > item.OnHandAmount)
            throw new BadRequestException($"Reserved + Available cannot exceed on-hand amount (item {item.Id}).");
    }
}
