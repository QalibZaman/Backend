using Mapster;
using MediatR;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.IOrderService;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetFilteredOrdersQuery(GetFilteredOrdersModel Model, int ShopId, string? SearchTerm, PaginationRequest PageModel) : IRequest<PaginatedResponse<OrderListResponse>>;

public class GetFilteredOrdersQueryHandler(
    IOrderRepository orderRepository,
    IFilteredOrderService filteredOrderService) : IRequestHandler<GetFilteredOrdersQuery, PaginatedResponse<OrderListResponse>>
{
    public async Task<PaginatedResponse<OrderListResponse>> Handle(GetFilteredOrdersQuery request, CancellationToken cancellationToken)
    {
        var specification = filteredOrderService.BuildFilteredOrdersSpecification(request.Model, request.ShopId, request.SearchTerm, request.PageModel);

        var (items, totalAmount) = await orderRepository.GetAllPaginatedAsync(specification, cancellationToken);

        var responseCollection = items.Adapt<ICollection<OrderListResponse>>();

        var result = new PaginatedResponse<OrderListResponse>
        {
            Items = responseCollection,
            TotalAmount = totalAmount
        };

        return result;
    }
}