using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;

namespace NettoRetail.Application.Services.IOrderService;

public interface IOrderService
{
    Task<PrintOrderDetailsModel> BuildPrintOrderDetailsModelAsync(OrderEntity order);

    Task<List<OrderItemResponse>> GetOrderItemsAsync(OrderEntity order, CancellationToken cancellationToken);
}