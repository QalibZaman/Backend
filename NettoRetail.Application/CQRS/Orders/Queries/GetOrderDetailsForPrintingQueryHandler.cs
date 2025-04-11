using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetOrderDetailsForPrintingQuery(int OrderId) : IRequest<OrderPrintingResponse>;

public class GetOrderDetailsForPrintingQueryHandler(IOrderRepository orderRepository, IInventoryItemRepository inventoryItemRepository) : IRequestHandler<GetOrderDetailsForPrintingQuery, OrderPrintingResponse>
{
    public async Task<OrderPrintingResponse> Handle(GetOrderDetailsForPrintingQuery request, CancellationToken cancellationToken)
    {
        var orderSpecification = new GetOrderByIdSpecification(request.OrderId);

        // Specification Pattern implementation does not implement ThenInclude method, so I am forced right now to implement a method specific for OrderRepository.
        // Implementing ThenInclude myself will require refactoring in a lot of places, so for the sake of implementing this functionality, let's leave this temporarily how it is.

        var order = await orderRepository.GetOrderForPrinting(orderSpecification, cancellationToken);

        if (order == null)
        {
            throw new NotFoundException("Order can't be found");
        }

        var response = order.Adapt<OrderPrintingResponse>();

        response.TotalProducts = order.OrderItems.Count();
        response.Subtotal = order.OrderItems.Aggregate(0m, (curr, next) => curr + (next.Quantity * next.InventoryItem.Price));

        response.TotalSale = order.OrderItems.Aggregate(0m, (curr, next) =>
            next.InventoryItem.PromotionPrice != null
                ? curr + ((next.InventoryItem.Price - next.InventoryItem.PromotionPrice.Value) * next.Quantity)
                : curr);

        response.TotalAmount = response.Subtotal - response.TotalSale;

        return order.Adapt<OrderPrintingResponse>();
    }
}
