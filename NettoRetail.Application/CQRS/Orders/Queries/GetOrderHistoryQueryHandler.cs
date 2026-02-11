using Mapster;
using MediatR;
using NettoRetail.Application.Models.Order;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetOrderHistoryQuery(int OrderId, int ShopId) : IRequest<IEnumerable<OrderHistoryResponse>>;

public class GetOrderHistoryQueryHandler(IOrderStatusHistoryRepository orderStatusHistoryRepository)
    : IRequestHandler<GetOrderHistoryQuery, IEnumerable<OrderHistoryResponse>>
{
    public async Task<IEnumerable<OrderHistoryResponse>> Handle(GetOrderHistoryQuery request, CancellationToken cancellationToken)
    {
        var history = await orderStatusHistoryRepository.GetOrderHistoryIncludingAccount(request.OrderId);

        var response = history.Adapt<IEnumerable<OrderHistoryResponse>>();

        return response;
    }
}