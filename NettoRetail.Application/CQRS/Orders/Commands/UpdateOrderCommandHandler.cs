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

        var order = await orderRepository.GetAsync(specificationBuilder.Build(), cancellationToken) ?? throw new NotFoundException("There is no such order.");

        if (order.OrderStatus == OrderStatusEnum.InProgress)
        {
            throw new BadRequestException("This order cannot be edited while in 'In Progress' status.");
        }

        var inventoryItemsIds = request.Model.ItemsToUpsert.Keys.Concat(request.Model.ItemsToDelete);

        var getItemsByIdsSpecification = new GetItemsByIdsSpecification(inventoryItemsIds);
        var inventoryItems = await inventoryItemRepository.GetManyBySpecAsync(getItemsByIdsSpecification, cancellationToken) ?? throw new NotFoundException("There are no such items.");

        var itemsList = new List<InventoryItemEntity>();

        var customerSpecification = new GetCustomerByIdSpecification(request.Model.CustomerId);
        var customer = await customerRepository.GetAsync(customerSpecification, cancellationToken) ?? throw new NotFoundException("There is no such customer.");

        var transaction = unitOfWork.BeginTransaction();

        try
        {
            request.Model.Adapt(order);

            foreach (var itemToUpsert in request.Model.ItemsToUpsert)
            {
                var item = inventoryItems.First(x => x.Id == itemToUpsert.Key);

                if (order.OrderItems.Select(x => x.ItemId).Contains(itemToUpsert.Key))
                {
                    var orderItem = order.OrderItems.First(x => x.ItemId == itemToUpsert.Key);

                    item.AvailableAmount -= itemToUpsert.Value - orderItem.Quantity;
                    item.ReservedAmount += itemToUpsert.Value - orderItem.Quantity;

                    orderItem.Quantity = itemToUpsert.Value;
                }
                else
                {
                    order.OrderItems.Add(new OrderItemEntity { ItemId = itemToUpsert.Key, Quantity = itemToUpsert.Value, OrderId = request.OrderId });

                    item.AvailableAmount -= itemToUpsert.Value;
                    item.ReservedAmount += itemToUpsert.Value;
                }
            }

            foreach (var itemIdToDelete in request.Model.ItemsToDelete)
            {
                var item = inventoryItems.First(x => x.Id == itemIdToDelete);
                var itemToRemove = order.OrderItems.Where(x => x.ItemId == itemIdToDelete).FirstOrDefault()
                    ?? throw new NotFoundException("One of the inventory items to delete is inaccessible");

                item.AvailableAmount += itemToRemove.Quantity;
                item.ReservedAmount -= itemToRemove.Quantity;

                order.OrderItems.Remove(itemToRemove);
            }

            await inventoryItemRepository.UpdateManyAsync(itemsList, cancellationToken);

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
}