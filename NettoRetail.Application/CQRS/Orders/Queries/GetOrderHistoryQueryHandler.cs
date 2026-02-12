using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetOrderHistoryQuery(Guid CustomerId, int shopId) : IRequest<IEnumerable<OrderStatusHistoryResponse>>;

public class GetOrderHistoryQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrderHistoryQuery, IEnumerable<OrderStatusHistoryResponse>>
{
    public async Task<IEnumerable<OrderStatusHistoryResponse>> Handle(GetOrderHistoryQuery request, CancellationToken cancellationToken)
    {
        var ordersByCustomerIdShopId = new GetOrderByCustomerIdAndShopIdSpecification(request.CustomerId, request.shopId);
        var order = await orderRepository.GetManyBySpecAsync(ordersByCustomerIdShopId, cancellationToken) ?? throw new NotFoundException("There is no such order.");

        var response = order.Adapt<IEnumerable<OrderStatusHistoryResponse>>();

        return response;
    }
}