using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetOrderHistoryQuery(Guid CustomerId) : IRequest<IEnumerable<OrderHistoryResponse>>;

public class GetOrderHistoryQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrderHistoryQuery, IEnumerable<OrderHistoryResponse>>
{
    public async Task<IEnumerable<OrderHistoryResponse>> Handle(GetOrderHistoryQuery request, CancellationToken cancellationToken)
    {
        var ordersByIdSpecification = new GetOrdersByCustomerIdSpecification(request.CustomerId);
        var order = await orderRepository.GetManyBySpecAsync(ordersByIdSpecification, cancellationToken)
                    ?? throw new NotFoundException("There is no such order.");

        var response = order.Adapt<IEnumerable<OrderHistoryResponse>>();

        return response;
    }
}