using NettoRetail.Application.Models.Request.Order;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Services.IOrderService.OrderStatusService
{
    public interface IOrderStatusService
    {
        Task UpdateOrderStatusAsync(OrderEntity order, UpdateOrderRequest model, Guid updatedBy, CancellationToken cancellation);
    }
}