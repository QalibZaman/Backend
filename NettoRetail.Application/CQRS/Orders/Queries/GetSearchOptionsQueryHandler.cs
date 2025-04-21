using MediatR;
using Microsoft.EntityFrameworkCore;
using NettoRetail.Application.Models.Order;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetSearchOptionsQuery(SearchFieldModel Search, int ShopId) : IRequest<List<string>>;

public class GetSearchOptionsQueryHandler(IOrderRepository orderRepository) : IRequestHandler<GetSearchOptionsQuery, List<string>>
{
    public async Task<List<string>> Handle(GetSearchOptionsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Search.SearchTerm))
            return [];

        var field = request.Search.Field.ToLower().Trim();
        var term = request.Search.SearchTerm.Trim();
        var builder = new SpecificationBuilder<OrderEntity>();

        builder.AddCriteria(x => x.ShopId == request.ShopId);

        switch (field)
        {
            case "creator":
                builder.AddCriteria(x => EF.Functions.ILike(x.Creator, $"%{term}%"));
                break;

            case "customerfirstname":
                builder
                    .AddInclude(x => x.Customer.Account)
                    .AddCriteria(x => EF.Functions.ILike(x.Customer.Account.FirstName, $"%{term}%"));
                break;

            case "customerlastname":
                builder
                    .AddInclude(x => x.Customer.Account)
                    .AddCriteria(x => EF.Functions.ILike(x.Customer.Account.LastName, $"%{term}%"));
                break;

            case "customeremail":
                builder
                    .AddInclude(x => x.Customer.Account)
                    .AddCriteria(x => EF.Functions.ILike(x.Customer.Account.Email, $"%{term}%"));
                break;

            case "orderid":
                builder.AddCriteria(x => EF.Functions.ILike(x.Id.ToString(), $"%{term}%"));
                break;

            case "customerid":
                builder.AddCriteria(x => EF.Functions.ILike(x.CustomerId.ToString(), $"%{term}%"));
                break;

            default:
                return [];
        }

        var spec = builder.Build();
        var results = await orderRepository.GetAllBySpecAsync(spec, cancellationToken);

        Func<OrderEntity, string?> selector = field switch
        {
            "creator" => x => x.Creator,
            "customerfirstname" => x => x.Customer.Account.FirstName,
            "customerlastname" => x => x.Customer.Account.LastName,
            "customeremail" => x => x.Customer.Account.Email,
            "orderid" => x => x.Id.ToString(),
            "customerid" => x => x.CustomerId.ToString(),
            _ => _ => null!
        };
        return results
            .Select(selector)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .OrderBy(x => x)
            .Select(x => $"{x}")
            .ToList();
    }
}