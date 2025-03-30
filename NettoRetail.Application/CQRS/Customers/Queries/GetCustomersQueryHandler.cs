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

public record GetCustomersQuery(string? Name, 
    string? Email,
    string? LoyaltyId,
    int? Page,
    int? PageSize) : IRequest<PaginatedResponse<CustomerResponse>>;

public class GetCustomersQueryHandler(ICustomerRepository customerRepository, IOptions<PaginationOptions> paginationOptions) : IRequestHandler<GetCustomersQuery, PaginatedResponse<CustomerResponse>>
{
    public async Task<PaginatedResponse<CustomerResponse>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<CustomerEntity>();

        var pageSize = request.PageSize ?? paginationOptions.Value.PageSize;
        var page = (request.Page - 1) * pageSize ?? 0;

        specificationBuilder.Add<GetCustomerByEmailSpecification>(request.Email)
                .Add<GetCustomerByNameSpecification>(request.Name)
                .Add<GetCustomerByLoyaltyIdSpecification>(request.LoyaltyId);

        specificationBuilder.AddInclude(x => x.Account)
            .Skip(page)
            .Take(pageSize);

        var specification = specificationBuilder.Build();

        var customers = await customerRepository.GetCustomersPaginatedAsync(specification, cancellationToken);
        var (items, totalAmount) = customers;

        var mappedCustomers = items.Adapt<ICollection<CustomerResponse>>();

        return new PaginatedResponse<CustomerResponse>()
        {
            Items = mappedCustomers,
            TotalAmount = totalAmount
        };
    }
}