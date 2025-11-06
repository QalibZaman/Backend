using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetSearchOptionsQuery(SearchFieldModel Search, int ShopId) : IRequest<ICollection<SearchOrderResponse>>;

public class GetSearchOptionsQueryHandler(IOrderRepository orderRepository) : IRequestHandler<GetSearchOptionsQuery, ICollection<SearchOrderResponse>>
{
    public async Task<ICollection<SearchOrderResponse>> Handle(GetSearchOptionsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Search.SearchTerm))
            return [];

        var field = request.Search.Field.ToLower().Trim();
        var term = request.Search.SearchTerm.Trim();
        var builder = new SpecificationBuilder<OrderEntity>();

        builder.AddCriteria(x => x.ShopId == request.ShopId);

        switch (field)
        {
            case "customeremail":
                builder
                    .AddInclude(x => x.Customer.Account)
                    .AddCriteria(x => EF.Functions.ILike(x.Customer.Account.Email, $"%{term}%"));
                break;

            case "orderid":
                builder
                    .AddCriteria(x => EF.Functions.ILike(x.Id.ToString(), $"%{term}%"));
                break;

            default:
                return [];
        }

        var spec = builder.Build();
        var results = await orderRepository.GetAllBySpecAsync(spec, cancellationToken);
        ICollection<SearchOrderResponse> AdaptSearchOrderResponse(IEnumerable<OrderEntity> entities)
        {
            List<SearchOrderResponse> responseList = new List<SearchOrderResponse>(capacity: entities.Count());
            foreach (var entity in entities)
            {
                var response = new SearchOrderResponse
                {
                    Id = entity.Id,
                    CreationDate = entity.CreationDate,
                    FullName = entity.Customer.Account.FullName,
                    Email = entity.Customer.Account.Email,
                    OrderStatus = entity.OrderStatus,
                    Creator = entity.Creator
                };
                responseList.Add(response);
            }

            return responseList;
        }

        var response = AdaptSearchOrderResponse(results);
        return response;
    }
}