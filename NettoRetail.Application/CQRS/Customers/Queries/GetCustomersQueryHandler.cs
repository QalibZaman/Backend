using Mapster;
using MediatR;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Customers.Queries;

public record GetCustomersQuery(GetCustomersRequest Model) : IRequest<PaginatedResponse<CustomerResponse>>;

public class GetCustomersQueryHandler(ICustomerRepository customerRepository) : IRequestHandler<GetCustomersQuery, PaginatedResponse<CustomerResponse>>
{
    public async Task<PaginatedResponse<CustomerResponse>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<CustomerEntity>();
        var pageSize = request.Model?.PageSize ?? PaginationConstants.DefaultPageSize;
        var skip = request.Model?.Page * pageSize ?? 0;

        specificationBuilder.Add<GetCustomerByEmailSpecification>(request.Model?.Email)
                .Add<GetCustomerByNameSpecification>(request.Model?.Name)
                .Add<GetCustomerByLoyaltyIdSpecification>(request.Model?.LoyaltyId);

        specificationBuilder.AddInclude(x => x.Account)
            .Skip(skip)
            .Take(pageSize);

        var specification = specificationBuilder.Build();

        var customers = await customerRepository.GetAllPaginatedAsync(specification, cancellationToken);
        var (items, totalAmount) = customers;

        var mappedCustomers = items.Adapt<ICollection<CustomerResponse>>();

        return new PaginatedResponse<CustomerResponse>()
        {
            Items = mappedCustomers,
            TotalAmount = totalAmount
        };
    }
}