using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Orders.Commands;

public record UpdateOrderStatusCommand(int OrderId, OrderStatusEnum Status) : IRequest<OrderResponse>;

public class UpdateOrderStatusCommandHandler(IOrderRepository orderRepository) : IRequestHandler<UpdateOrderStatusCommand, OrderResponse>
{
    public async Task<OrderResponse> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetOrderByIdSpecification(request.OrderId);

        specification.AddInclude(o => o.Customer).AddInclude(o => o.Customer.Account);

        var order = await orderRepository.GetAsync(specification, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException("Order does not exist");
        }

        if (order.OrderStatus == request.Status)
        {
            throw new BadRequestException("Old and new statuses can not be the same");
        }

        if (order.StatusHistory == null)
        {
            order.StatusHistory = new List<OrderStatusHistoryEntity>();
        }

        var statusChange = new OrderStatusHistoryEntity()
        {
            OrderId = order.Id,
            OldStatus = order.OrderStatus,
            NewStatus = request.Status,
            UpdatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
        };

        order.StatusHistory.Add(statusChange);
        order.OrderStatus = request.Status;

        await orderRepository.UpdateAsync(order, cancellationToken);

        var response = order.Adapt<OrderResponse>();

        return response;
    }
}
