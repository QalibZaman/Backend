using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetOrderProductsAndPaymentsQuery(int OrderId) : IRequest<OrderProductsResponse>;
public class GetOrderProductsAndPaymentsQueryHandler(
    IOrderRepository orderRepository)
    : IRequestHandler<GetOrderProductsAndPaymentsQuery, OrderProductsResponse>
{
    public async Task<OrderProductsResponse> Handle(GetOrderProductsAndPaymentsQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetOrderByIdSpecification(request.OrderId);

        var order = await orderRepository.GetOrderWithInventoryItemsAsync(spec, cancellationToken)
            ?? throw new NotFoundException(HandlerConstants.Order.OrderNotFoundMessage);

        return order.Adapt<OrderProductsResponse>();

    }
}