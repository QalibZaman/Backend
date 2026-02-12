using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response.Order;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetOrderHistoryQuery(Guid CustomerId, int shopId) : IRequest<IEnumerable<OrderHistoryResponse>>;

public class GetOrderHistoryQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrderHistoryQuery, IEnumerable<OrderHistoryResponse>>
{
    public async Task<IEnumerable<OrderHistoryResponse>> Handle(GetOrderHistoryQuery request, CancellationToken cancellationToken)
    {
        var ordersByCustomerIdShopId = new GetOrderByCustomerIdAndShopIdSpecification(request.CustomerId, request.shopId);
        var order = await orderRepository.GetManyBySpecAsync(ordersByCustomerIdShopId, cancellationToken)
            ?? throw new NotFoundException("There is no such order.");

        var response = order.Adapt<IEnumerable<OrderHistoryResponse>>();

        return response;
    }
}