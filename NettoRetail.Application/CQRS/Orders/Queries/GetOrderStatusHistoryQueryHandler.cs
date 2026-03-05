using Mapster;
using MediatR;
using NettoRetail.Application.Models.Order;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetOrderStatusHistoryQuery(int OrderId, int ShopId) : IRequest<IEnumerable<OrderStatusHistoryResponse>>;

public class GetOrderStatusHistoryQueryHandler(
    IOrderStatusHistoryRepository orderStatusHistoryRepository) : IRequestHandler<GetOrderStatusHistoryQuery, IEnumerable<OrderStatusHistoryResponse>>
{
    public async Task<IEnumerable<OrderStatusHistoryResponse>> Handle(GetOrderStatusHistoryQuery request, CancellationToken cancellationToken)
    {
        var history = await orderStatusHistoryRepository.GetOrderHistoryIncludingAccount(request.OrderId, cancellationToken);

        var response = history.Adapt<IEnumerable<OrderStatusHistoryResponse>>();

        return response;
    }
}