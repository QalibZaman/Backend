using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using static NettoRetail.Application.Models.Order.SearchFieldModel;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetSearchOptionsQuery(SearchFieldModel Search, int ShopId) : IRequest<ICollection<SearchOrderResponse>>;

public class GetSearchOptionsQueryHandler(IOrderRepository orderRepository) : IRequestHandler<GetSearchOptionsQuery, ICollection<SearchOrderResponse>>
{
    public async Task<ICollection<SearchOrderResponse>> Handle(GetSearchOptionsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Search.SearchTerm))
            return [];

        var term = request.Search.SearchTerm.Trim();

        if (!Enum.TryParse<SearchCriteria>(request.Search.Field, true, out var criteria))
        {
            return [];
        }

        var builder = new SpecificationBuilder<OrderEntity>();

        builder.AddCriteria(x => x.ShopId == request.ShopId);
        builder.AddInclude(x => x.Customer);

        switch (criteria)
        {
            case SearchFieldModel.SearchCriteria.CustomerEmail:
                builder
                    .AddCriteria(x => EF.Functions.ILike(x.Customer.Email, $"%{term}%"));
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
        var response = results.Adapt<ICollection<SearchOrderResponse>>();
        return response;
    }
}