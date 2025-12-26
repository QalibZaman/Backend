using NettoRetail.Application.Models.Request.Order;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
namespace NettoRetail.Application.Services.IOrderService.OrderStatusService
{
    public interface IOrderStatusService
    {
        void EnforceOrderStatusRules(OrderStatusEnum currentStatus, OrderStatusEnum newStatus, string? comment);
        Task UpdateOrderStatusAsync(OrderEntity order, UpdateOrderRequest model, CancellationToken cancellation);
    }
}