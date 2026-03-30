using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetOrderGeneralInfoQuery(int OrderId) : IRequest<OrderGeneralInfoResponse>;

public class GetOrderGeneralInfoQueryHandler(
    IOrderRepository orderRepository,
    IMapper mapper)
    : IRequestHandler<GetOrderGeneralInfoQuery, OrderGeneralInfoResponse>
{
    public async Task<OrderGeneralInfoResponse> Handle(GetOrderGeneralInfoQuery request, CancellationToken cancellationToken)
    {
        if (request.OrderId <= 0)
            throw new BadRequestException("Order Id must be greater than zero.");

        var spec = new SpecificationBuilder<OrderEntity>()
            .Add<GetOrderByIdSpecification>(request.OrderId)
            .AddInclude(x => x.Customer)
            .Build();

        var order = await orderRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException("Order can't be found");

        var response = mapper.Map<OrderGeneralInfoResponse>(order);

        return response;
    }
}
