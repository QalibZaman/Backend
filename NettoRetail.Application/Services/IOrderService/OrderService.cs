using MapsterMapper;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.InventoryItem;
using NettoRetail.Application.Models.Order;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services.IOrderService;

public class OrderService(IOrderRepository orderRepository, IMapper mapper) : IOrderService
{
    public async Task<PrintOrderDetailsModel> BuildPrintOrderDetailsModelAsync(OrderEntity order)
    {
        var model = mapper.Map<PrintOrderDetailsModel>(order);
        model.TotalProducts = order.OrderItems.Count;
        model.Subtotal = order.OrderItems.Sum(x => x.Quantity * x.InventoryItem.Price);
        model.TotalSale = order.OrderItems.Sum(x =>
            x.InventoryItem.PromotionPrice != null
                ? (x.InventoryItem.Price - x.InventoryItem.PromotionPrice.Value) * x.Quantity
                : 0);

        model.TotalAmount = model.Subtotal - model.TotalSale;
        return model;
    }

    public async Task<OrderEntity> GetOrderAsync(int orderId, CancellationToken ct)
    {
        var spec = new GetOrderByIdSpecification(orderId)
            .AddInclude(o => o.Customer)
            .AddInclude(o => o.OrderItems);

        return await orderRepository.GetAsync(spec, ct)
            ?? throw new NotFoundException("Order does not exist");
    }

}
