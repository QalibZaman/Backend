using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetOrderProductsQuery(int OrderId, int UserShopId) : IRequest<OrderProductsResponse>;
public class GetOrderProductsQueryHandler(
    IOrderRepository orderRepository,
    IMapper mapper)
    : IRequestHandler<GetOrderProductsQuery, OrderProductsResponse>
{
    public async Task<OrderProductsResponse> Handle(GetOrderProductsQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetOrderByIdAndShopIdSpecification(request.OrderId, request.UserShopId);

        var order = await orderRepository.GetOrderWithInventoryItemsAsync(spec, cancellationToken)
            ?? throw new NotFoundException(HandlerConstants.Order.OrderNotFoundMessage);

        var response = mapper.Map<OrderProductsResponse>(order);

        return response;
    }
}
