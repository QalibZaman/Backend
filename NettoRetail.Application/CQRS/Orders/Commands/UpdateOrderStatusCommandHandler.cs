using Azure.Core;
using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Order;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Orders.Commands;

public record UpdateOrderStatusCommand(UpdateOrderRequest Model) : IRequest<OrderResponse>;

public class UpdateOrderStatusCommandHandler(IOrderRepository orderRepository, IInventoryItemRepository inventoryItemRepository, IUnitOfWork unitOfWork) : IRequestHandler<UpdateOrderStatusCommand, OrderResponse>
{
    public async Task<OrderResponse> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetOrderByIdSpecification(request.Model.OrderId);

        specification.AddInclude(o => o.Customer)
            .AddInclude(o => o.Customer.Account)
            .AddInclude(o => o.OrderItems);

        var order = await orderRepository.GetAsync(specification, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException("Order does not exist");
        }

        EnforceOrderStatusRules(order.OrderStatus, request.Model.Status, request.Model.Comment);

        if (order.StatusHistory == null)
        {
            order.StatusHistory = new List<OrderStatusHistoryEntity>();
        }

        var statusChange = new OrderStatusHistoryEntity()
        {
            OrderId = order.Id,
            OldStatus = order.OrderStatus,
            NewStatus = request.Model.Status,
            UpdatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            Comment = request.Model.Comment
        };

        order.StatusHistory.Add(statusChange);

        var transaction = unitOfWork.BeginTransaction();

        try
        {
            if (request.Model.Status == OrderStatusEnum.InProgress || (request.Model.Status == OrderStatusEnum.Cancelled && order.OrderStatus == OrderStatusEnum.InProgress))
            {
                var itemIds = order.OrderItems.Select(i => i.ItemId);
                var itemsSpecification = new GetItemsByIdsSpecification(itemIds!);
                var inventoryItems = await inventoryItemRepository.GetAllBySpecAsync(itemsSpecification, cancellationToken);

                UpdateInventoryItemsAmount(inventoryItems!, order.OrderItems, request.Model.Status);

                await inventoryItemRepository.UpdateManyAsync(inventoryItems, cancellationToken);
            }

            order.OrderStatus = request.Model.Status;
            await orderRepository.UpdateAsync(order, cancellationToken);
            transaction.Commit();
        } catch
        {
            transaction.Rollback();
            throw;
        }

        var response = order.Adapt<OrderResponse>();

        return response;
    }

    private static void EnforceOrderStatusRules(OrderStatusEnum currentStatus, OrderStatusEnum newStatus, string? comment)
    {
        if (currentStatus == newStatus)
        {
            throw new BadRequestException("Old and new statuses can not be the same");
        }

        if (currentStatus == OrderStatusEnum.Done || currentStatus == OrderStatusEnum.Cancelled)
        {
            throw new BadRequestException("Order status can not be edited if the status is Done or Cancelled");
        }

        if ((newStatus == OrderStatusEnum.Processing || newStatus == OrderStatusEnum.Cancelled) && string.IsNullOrEmpty(comment))
        {
            throw new BadRequestException("Comment can't be empty when changing status to Processing or Cancelled");
        }

        if (currentStatus == OrderStatusEnum.InProgress && newStatus == OrderStatusEnum.Processing && string.IsNullOrEmpty(comment))
        {
            throw new BadRequestException("Comment can't be empty when changing status from In Progress to Processing");
        }

        if (currentStatus == OrderStatusEnum.Processing && newStatus == OrderStatusEnum.New)
        {
            throw new BadRequestException("Changing status from Processing to New is prohibited");
        }
    }

    private static void UpdateInventoryItemsAmount(IEnumerable<InventoryItemEntity> inventoryItems, IEnumerable<OrderItemEntity> orderItems, OrderStatusEnum orderStatus)
    {
        foreach (var item in orderItems)
        {
            var inventoryItem = inventoryItems.First(i => i.Id == item.ItemId);

            switch (orderStatus)
            {
                case OrderStatusEnum.InProgress:
                    inventoryItem.ReservedAmount -= item.Quantity;
                    inventoryItem.OnHandAmount -= item.Quantity;
                    break;
                case OrderStatusEnum.Cancelled:
                    inventoryItem.ReservedAmount += item.Quantity;
                    inventoryItem.OnHandAmount += item.Quantity;
                    break;
            }
        }
    }

}
