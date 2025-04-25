using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Order;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetSearchOrdersQuery(int Id, Guid CustomerId, string SearchTerm, int ShopId) : IRequest<IEnumerable<OrderSearchResponse>>;

public class GetSearchOrdersQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetSearchOrdersQuery, IEnumerable<OrderSearchResponse>>
{
    public async Task<IEnumerable<OrderSearchResponse>> Handle(GetSearchOrdersQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<OrderEntity>();
        specificationBuilder.Add<GetOrderSearchSpecification>(request.SearchTerm, request.ShopId)
                            .AddInclude(order => order.Customer)
                            .Add<GetOrdersByIdSpecification>()
                            .AddOrderBy(o => o.CreationDate, true);

        var orders = await orderRepository.GetAsync(specificationBuilder.Build(), cancellationToken) ?? throw new NotFoundException("Some message");
        var result = orders?.Adapt<List<OrderSearchResponse>>() ?? new List<OrderSearchResponse>();

        return result;
    }
}
