using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.OrderItems;
using NettoRetail.DataAccess.Specifications.Orders;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetOrderDetailsQuery(int OrderId) : IRequest<OrderResponse>;
public class GetOrderDetailsQueryHandler(
    IOrderRepository orderRepository,
    IOrderItemRepository orderItemRepository)
    : IRequestHandler<GetOrderDetailsQuery, OrderResponse>
{
    public async Task<OrderResponse> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken)
    {
        var ordersByIdSpecification = new GetOrdersByIdSpecification(request.OrderId);
        var order = await orderRepository.GetAsync(ordersByIdSpecification, cancellationToken)
            ?? throw new NotFoundException("There is no such order.");

        var orderItemSpecification = new GetOrderItemsByOrderIdSpecification(request.OrderId);
        var orderItems = await orderItemRepository.GetAllBySpecAsync(orderItemSpecification, cancellationToken);

        var items = orderItems.Adapt<List<OrderItemResponse>>();
        CalculateItemTotals(items);

        var response = order.Adapt<OrderResponse>();

        response.OrderItems = items;
        response.TotalSale = orderItems
            .Where(x => x.InventoryItem.IsPromoted && x.InventoryItem.PromotionPrice.HasValue)
            .Sum(x => x.InventoryItem.PromotionPrice!.Value);

        return response;
    }

    private static void CalculateItemTotals(List<OrderItemResponse> items)
    {
        foreach (var item in items)
        {
            item.Total = Math.Round(item.Price * item.Quantity, 2);
        }
    }
}