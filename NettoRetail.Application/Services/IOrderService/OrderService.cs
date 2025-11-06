using Mapster;
using NettoRetail.Application.Models.Order;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Services.IOrderService;

public class OrderService : IOrderService
{
    public async Task<PrintOrderDetailsModel> BuildPrintOrderDetailsModelAsync(OrderEntity order)
    {
        var model = order.Adapt<PrintOrderDetailsModel>();

        model.TotalProducts = order.OrderItems.Count;
        model.Subtotal = order.OrderItems.Sum(x => x.Quantity * x.InventoryItem.Price);
        model.TotalSale = order.OrderItems.Sum(x =>
            x.InventoryItem.PromotionPrice != null
                ? (x.InventoryItem.Price - x.InventoryItem.PromotionPrice.Value) * x.Quantity
                : 0);

        model.TotalAmount = model.Subtotal - model.TotalSale;

        return model;
    }
}
