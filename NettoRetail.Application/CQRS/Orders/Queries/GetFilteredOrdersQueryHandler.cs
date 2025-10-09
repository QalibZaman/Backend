using System.Linq.Expressions;
using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetFilteredOrdersQuery(GetFilteredOrdersModel Model, int ShopId, string? SearchTerm, PaginationRequest PageModel) : IRequest<PaginatedResponse<OrderListResponse>>;

public class GetFilteredOrdersQueryHandler(IOrderRepository orderRepository) : IRequestHandler<GetFilteredOrdersQuery, PaginatedResponse<OrderListResponse>>
{
    public async Task<PaginatedResponse<OrderListResponse>> Handle(GetFilteredOrdersQuery request, CancellationToken cancellationToken)
    {
        var specBuilder = new SpecificationBuilder<OrderEntity>();
        var skip = (request.PageModel.PageNumber - 1) * request.PageModel.PageSize;
        var startDate = request.Model.StartDate.HasValue
            ? DateTime.SpecifyKind(request.Model.StartDate.Value, DateTimeKind.Utc)
            : (DateTime?)null;

        var endDate = request.Model.EndDate.HasValue
            ? DateTime.SpecifyKind(request.Model.EndDate.Value, DateTimeKind.Utc)
            : (DateTime?)null;

        specBuilder.Add<GetOrdersByShopIdSpecification>(request.ShopId);

        specBuilder.Add<GetOrdersByCustomerIdsSpecification>(request.Model.CustomerIds);
        specBuilder.Add<GetOrdersByFirstNamesSpecification>(request.Model.CustomerFirstNames);
        specBuilder.Add<GetOrdersByLastNamesSpecification>(request.Model.CustomerLastNames);
        specBuilder.Add<GetOrdersByEmailsSpecification>(request.Model.CustomerEmails);
        specBuilder.Add<GetOrdersByOrderIdsSpecification>(request.Model.OrderIds);
        specBuilder.Add<GetOrdersByCreatorSpecification>(request.Model.Creators);
        specBuilder.Add<GetOrdersByDateRangeSpecification>(startDate, endDate);

        if (request.Model.OrderStatuses != null)
        {
            var enums = request.Model.OrderStatuses.Select(s => Enum.Parse<OrderStatusEnum>(s, true)).ToList();
            specBuilder.Add<GetOrdersByStatusEnumSpecification>(enums);
        }

        if (request.Model.DeliveryTypes != null)
        {
            var enums = request.Model.DeliveryTypes.Select(t => Enum.Parse<OrderDeliveryTypeEnum>(t, true)).ToList();
            specBuilder.Add<GetOrdersByDeliveryTypeEnumSpecification>(enums);
        }

        if (request.Model.SortingOptions != null)
        {
            foreach (var option in request.Model.SortingOptions)
            {
                specBuilder.AddOrderBy(GetSortingExpression(option.Field), option.IsDescending);
            }
        }
        else
        {
            specBuilder.AddOrderBy(o => o.CreationDate, true);
        }

        if (request.SearchTerm != null)
        {
            specBuilder.Add<GetOrderSearchSpecification>(request.SearchTerm);
        }

        specBuilder.AddInclude(x => x.Customer.Account);

        var specification = specBuilder.Skip(skip).Take(request.PageModel.PageSize).Build();

        var (items, totalAmount) = await orderRepository.GetAllPaginatedAsync(specification, cancellationToken);

        if (items.Count == 0)
        {
            throw new NotFoundException("No orders found");
        }

        var responseCollection = items.Adapt<List<OrderListResponse>>();

        var responseModel = new PaginatedResponse<OrderListResponse> { Items = responseCollection, TotalAmount = totalAmount };

        return responseModel;
    }

    private static Expression<Func<OrderEntity, object>> GetSortingExpression(string fieldName) =>
        fieldName.ToLower() switch
        {
            "datecreated" => o => o.CreationDate,
            "orderid" => o => o.Id,
            "orderstatus" => o => o.OrderStatus,
            "deliverytype" => o => o.DeliveryType,
            "creator" => o => o.Creator,
            _ => o => o.CreationDate
        };
}
