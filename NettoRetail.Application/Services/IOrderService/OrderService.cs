using Mapster;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.OrderItems;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services.IOrderService;

public class OrderService(IOrderItemRepository orderItemRepository) : IOrderService
{
    private readonly IOrderItemRepository _orderItemRepository = orderItemRepository;
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

    public async Task<List<OrderItemResponse>> GetOrderItemsAsync(OrderEntity order, CancellationToken cancellationToken)
    {
        var orderItemSpecification = new GetOrderItemsByOrderIdSpecification(order.Id);
        orderItemSpecification.AddInclude(x => x.InventoryItem);

        var orderItems = await _orderItemRepository.GetAllBySpecAsync(orderItemSpecification, cancellationToken);

        var result = new List<OrderItemResponse>(orderItems.Count());
        foreach (var orderItem in orderItems)
        {
            var itemResponse = orderItem.Adapt<OrderItemResponse>();
            itemResponse.AvailableAmount = orderItem.InventoryItem.AvailableAmount;
            itemResponse.ProductName = orderItem.InventoryItem.ProductName;
            itemResponse.ProductUPC = orderItem.InventoryItem.ProductUpc.ToString();
            itemResponse.Price = orderItem.InventoryItem.Price;
            itemResponse.IsPromoted = orderItem.InventoryItem.IsPromoted;
            itemResponse.PromotionPrice = orderItem.InventoryItem?.PromotionPrice ?? 0.00m;
            itemResponse.SetTotal();
            result.Add(itemResponse);
        }

        return result;
    }
}
