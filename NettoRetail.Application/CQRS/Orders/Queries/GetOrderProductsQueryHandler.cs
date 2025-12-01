using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.IOrderService;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.DataAccess.Specifications.OrderItems;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetOrderProductsQuery(int OrderId) : IRequest<OrderProductsResponse>;
public class GetOrderProductsQueryHandler(
    IOrderRepository orderRepository,
    IOrderService orderService)
    : IRequestHandler<GetOrderProductsQuery, OrderProductsResponse>
{
    public async Task<OrderProductsResponse> Handle(GetOrderProductsQuery request, CancellationToken cancellationToken)
    {
        var ordersByIdSpecification = new GetOrdersByIdSpecification(request.OrderId);
        var order = await orderRepository.GetAsync(ordersByIdSpecification, cancellationToken)
            ?? throw new NotFoundException("There is no such order.");

        var inventoryItems = await orderService.GetOrderItemsAsync(order, cancellationToken);

        OrderProductsResponse response = order.Adapt<OrderProductsResponse>();
        response.OrderItems = inventoryItems;
        response.SetPrices();

        return response;
    }
}