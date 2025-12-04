using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetOrderProductsQuery(int OrderId) : IRequest<OrderProductsResponse>;
public class GetOrderProductsQueryHandler(
    IOrderRepository orderRepository)
    : IRequestHandler<GetOrderProductsQuery, OrderProductsResponse>
{
    public async Task<OrderProductsResponse> Handle(GetOrderProductsQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetOrderByIdSpecification(request.OrderId);

        var order = await orderRepository.GetOrderForProducts(spec, cancellationToken)
            ?? throw new NotFoundException("There is no such order.");

        var response = order.Adapt<OrderProductsResponse>();

        return response;
    }
}