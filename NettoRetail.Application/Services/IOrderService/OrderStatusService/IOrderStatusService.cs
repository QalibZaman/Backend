using NettoRetail.Application.Models.Request.Order;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
namespace NettoRetail.Application.Services.IOrderService.OrderStatusService
{
    public interface IOrderStatusService
    {
        Task UpdateOrderStatusAsync(OrderEntity order, UpdateOrderRequest model, CancellationToken cancellation);
    }
}