using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Order;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.Services.IOrderService.OrderStatusService
{
    public class OrderStatusService(
        IInventoryItemRepository itemRepository,
        IUnitOfWork unitOfWork,
        IOrderRepository orderRepository) : IOrderStatusService
    {
        public async Task UpdateOrderStatusAsync(OrderEntity order, UpdateOrderRequest model, CancellationToken cancellation)
        {
            order.StatusHistory ??= new List<OrderStatusHistoryEntity>();
            order.StatusHistory.Add(CreateStatusHistory(order, model));
            using var tx = unitOfWork.BeginTransaction();
            try
            {
                if (ShouldUpdateInventory(order.OrderStatus, model.Status))
                    await UpdateInventoryAsync(order, model.Status, cancellation);
                order.OrderStatus = model.Status;
                await orderRepository.UpdateAsync(order, cancellation);
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public void EnforceOrderStatusRules(OrderStatusEnum currentStatus, OrderStatusEnum newStatus, string? comment)
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

        private static OrderStatusHistoryEntity CreateStatusHistory(
        OrderEntity order,
        UpdateOrderRequest model)
        {
            return new OrderStatusHistoryEntity
            {
                OrderId = order.Id,
                OldStatus = order.OrderStatus,
                NewStatus = model.Status,
                UpdatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
                Comment = model.Comment
            };
        }

        private static bool ShouldUpdateInventory(
        OrderStatusEnum oldStatus,
        OrderStatusEnum newStatus)
        {
            return newStatus == OrderStatusEnum.InProgress
                || (newStatus == OrderStatusEnum.Cancelled && oldStatus == OrderStatusEnum.InProgress);
        }

        private static void ApplyInventoryChanges(
            IEnumerable<InventoryItemEntity> inventoryItems,
            IEnumerable<OrderItemEntity> orderItems,
            OrderStatusEnum newStatus)
        {
            foreach (var item in orderItems)
            {
                var inventoryItem = inventoryItems.First(i => i.Id == item.ItemId);
                if (newStatus == OrderStatusEnum.InProgress)
                {
                    inventoryItem.ReservedAmount -= item.Quantity;
                    inventoryItem.OnHandAmount -= item.Quantity;
                }
                else if (newStatus == OrderStatusEnum.Cancelled)
                {
                    inventoryItem.ReservedAmount += item.Quantity;
                    inventoryItem.OnHandAmount -= item.Quantity;
                }
            }
        }

        private async Task UpdateInventoryAsync(
        OrderEntity order,
        OrderStatusEnum newStatus,
        CancellationToken ct)
        {
            var itemIds = order.OrderItems.Select(i => i.ItemId);
            var spec = new GetItemsByIdsSpecification(itemIds);

            var inventoryItems = await itemRepository.GetAllBySpecAsync(spec, ct);

            ApplyInventoryChanges(inventoryItems!, order.OrderItems, newStatus);
            await itemRepository.UpdateManyAsync(inventoryItems!, ct);
        }
    }
}
