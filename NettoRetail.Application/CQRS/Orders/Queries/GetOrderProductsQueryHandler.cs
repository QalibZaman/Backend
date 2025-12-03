using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetOrderProductsQuery(int OrderId) : IRequest<OrderProductsResponse>;
public class GetOrderProductsQueryHandler(
    IOrderRepository orderRepository)
    : IRequestHandler<GetOrderProductsQuery, OrderProductsResponse>
{
    public async Task<OrderProductsResponse> Handle(GetOrderProductsQuery request, CancellationToken cancellationToken)
    {
        var spec = new SpecificationBuilder<OrderEntity>()
            .Add<GetOrderByIdSpecification>(request.OrderId)
            .Build();

        var order = await orderRepository.GetOrderForProducts(spec, cancellationToken)
            ?? throw new NotFoundException("There is no such order.");

        var response = order.Adapt<OrderProductsResponse>();

        response.Subtotal = response.OrderItems.Sum(i => i.Price * i.Quantity);
        response.Sale = response.OrderItems.Sum(i => i.IsPromoted ? i.PromotionPrice * i.Quantity : 0.0m);
        response.Total = response.OrderItems.Sum(i => i.Total);

        return response;
    }
}