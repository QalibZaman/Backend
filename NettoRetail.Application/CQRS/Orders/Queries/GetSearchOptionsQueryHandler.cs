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

        var criteria = SearchFieldModel.SearchCriteria.None;
        if (field == "customeremail") criteria = SearchFieldModel.SearchCriteria.CustomerEmail;
        if (field == "orderid") criteria = SearchFieldModel.SearchCriteria.OrderId;
        switch (criteria)
        {
            case SearchFieldModel.SearchCriteria.CustomerEmail:
                builder
                    .AddInclude(x => x.Customer.Account)
                    .AddCriteria(x => EF.Functions.ILike(x.Customer.Account.Email, $"%{term}%"));
                break;

            case SearchFieldModel.SearchCriteria.OrderId:
                builder
                    .AddCriteria(x => EF.Functions.ILike(x.Id.ToString(), $"%{term}%"));
                break;

            case SearchFieldModel.SearchCriteria.None:
                return [];
        }

        var spec = builder.Build();
        var results = await orderRepository.GetAllBySpecAsync(spec, cancellationToken);
        var response = SearchFieldModel.AdaptSearchOrderResponse(results);
        return response;
    }
}