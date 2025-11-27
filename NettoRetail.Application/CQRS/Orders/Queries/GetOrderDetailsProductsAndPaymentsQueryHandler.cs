using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.DataAccess.Specifications.OrderItems;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetOrderDetailsProductsAndPaymentsQuery(int OrderId) : IRequest<OrderProductsAndPaymentsResponse>;
public class GetOrderDetailsProductsAndPaymentsQueryHandler(
    IOrderRepository orderRepository,
    IOrderItemRepository orderItemRepository)
    : IRequestHandler<GetOrderDetailsProductsAndPaymentsQuery, OrderProductsAndPaymentsResponse>
{
    public async Task<OrderProductsAndPaymentsResponse> Handle(GetOrderDetailsProductsAndPaymentsQuery request, CancellationToken cancellationToken)
    {
        var ordersByIdSpecification = new GetOrdersByIdSpecification(request.OrderId);
        var order = await orderRepository.GetAsync(ordersByIdSpecification, cancellationToken)
            ?? throw new NotFoundException("There is no such order.");

        var orderItemSpecification = new GetOrderItemsByOrderIdSpecification(request.OrderId);
        var orderItems = await orderItemRepository.GetAllBySpecAsync(orderItemSpecification, cancellationToken);

        var items = OrderItemResponse.GetListOfResponses((List<Domain.Entities.JunctionEntities.OrderItemEntity>)orderItems);

        OrderProductsAndPaymentsResponse response = new();
        response.DeliveryType = order.DeliveryType.ToString();
        response.OrderStatus = order.OrderStatus;
        response.OrderItems = items;
        response.SetPrices();

        return response;
    }
}