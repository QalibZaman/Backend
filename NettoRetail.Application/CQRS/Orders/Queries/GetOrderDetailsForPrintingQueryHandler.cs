using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetOrderDetailsForPrintingQuery(int OrderId) : IRequest<OrderPrintingResponse>;

public class GetOrderDetailsForPrintingQueryHandler(IOrderRepository orderRepository, IInventoryItemRepository inventoryItemRepository) : IRequestHandler<GetOrderDetailsForPrintingQuery, OrderPrintingResponse>
{
    public async Task<OrderPrintingResponse> Handle(GetOrderDetailsForPrintingQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<OrderEntity>();
        var specification = specificationBuilder.Add<GetOrderByIdSpecification>(request.OrderId)
            .AddInclude(x => x.Customer)
            .AddInclude(x => x.Customer.Account)
            .AddInclude(x => x.StatusHistory)
            .AddInclude(x => x.OrderItems)
            .Build();

        // Specification Pattern implementation does not implement ThenInclude method, so I am forced right now to implement a method specific for OrderRepository.
        // Implementing ThenInclude myself will require refactoring in a lot of places, so for the sake of implementing this functionality, let's leave this temporarily how it is.

        var order = await orderRepository.GetOrderForPrinting(specification, cancellationToken);

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

        return response;
    }
}
