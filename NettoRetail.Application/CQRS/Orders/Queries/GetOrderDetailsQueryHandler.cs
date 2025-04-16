using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Orders;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetOrderDetailsQuery(int OrderId) : IRequest<OrderResponse>;
public class GetOrderDetailsQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrderDetailsQuery, OrderResponse>
{
    public async Task<OrderResponse> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken)
    {
        var ordersByIdSpecification = new GetOrdersByIdSpecification(request.OrderId);
        var order = await orderRepository.GetAsync(ordersByIdSpecification, cancellationToken) ?? throw new NotFoundException("There is no such order.");

        var response = order.Adapt<OrderResponse>();

        return response;
    }
}