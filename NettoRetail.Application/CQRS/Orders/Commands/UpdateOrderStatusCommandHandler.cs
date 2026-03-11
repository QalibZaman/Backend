using MapsterMapper;
using MediatR;
using NettoRetail.Application.Models.Request.Order;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.IOrderService;
using NettoRetail.Application.Services.IOrderService.OrderStatusService;
namespace NettoRetail.Application.CQRS.Orders.Commands;

public record UpdateOrderStatusCommand(UpdateOrderRequest Model, Guid userId) : IRequest<OrderResponse>;

public class UpdateOrderStatusCommandHandler(IOrderService orderService, IOrderStatusService orderStatusService, IMapper mapper) : IRequestHandler<UpdateOrderStatusCommand, OrderResponse>
{
    public async Task<OrderResponse> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await orderService.GetOrderAsync(request.Model.OrderId, cancellationToken);

        await orderStatusService.UpdateOrderStatusAsync(order, request.Model, request.userId, cancellationToken);

        var response = mapper.Map<OrderResponse>(order);

        return response;
    }
}
