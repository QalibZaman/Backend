using Mapster;
using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Customers.Queries;

public record GetCustomersQuery(string? SearchQuery, int? Page, int? PageSize) : IRequest<IEnumerable<CustomerResponse>>;

public class GetCustomersQueryHandler(ICustomerRepository customerRepository, IOptions<PaginationOptions> paginationOptions) : IRequestHandler<GetCustomersQuery, IEnumerable<CustomerResponse>>
{
    public async Task<IEnumerable<CustomerResponse>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<CustomerEntity>();

        var page = request.Page ?? 1;
        var pageSize = request.PageSize ?? paginationOptions.Value.PageSize;

        if (!string.IsNullOrEmpty(request.SearchQuery))
        {
            specificationBuilder.Add<GetCustomerByEmailSpecification>(request.SearchQuery)
                .Add<GetCustomerByPhoneNumberSpecification>(request.SearchQuery)
                .Add<GetCustomerByLoyaltyIdSpecification>(request.SearchQuery)
                .Skip(page)
                .Take(pageSize);
        }

        var specification = specificationBuilder.Build();

        var customers = await customerRepository.GetCustomersPaginatedAsync(specification, cancellationToken);

        return customers.Adapt<IEnumerable<CustomerResponse>>();
    }
}