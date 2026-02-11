using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.DataAccess.Specifications.Order.OrderStatusHistory;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetOrderHistoryQuery(int OrderId, int ShopId) : IRequest<IEnumerable<OrderHistoryResponse>>;

public class GetOrderHistoryQueryHandler(IOrderStatusHistoryRepository orderStatusHistoryRepository)
    : IRequestHandler<GetOrderHistoryQuery, IEnumerable<OrderHistoryResponse>>
{
    public async Task<IEnumerable<OrderHistoryResponse>> Handle(GetOrderHistoryQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetOrderStatusHistoryByIdSpecification(request.OrderId);

        var history = await orderStatusHistoryRepository.GetAllBySpecAsync(spec, cancellationToken);

        var response = history.Adapt<IEnumerable<OrderHistoryResponse>>();

        return response;
    }
}