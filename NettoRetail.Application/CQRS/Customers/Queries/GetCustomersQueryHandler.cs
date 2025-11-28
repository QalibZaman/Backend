using Mapster;
using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Customers.Queries;

public record GetCustomersQuery(PaginationRequest PageModel, GetCustomersRequest? Model) : IRequest<PaginatedResponse<CustomerResponse>>;

public class GetCustomersQueryHandler(ICustomerRepository customerRepository)
        : IRequestHandler<GetCustomersQuery, PaginatedResponse<CustomerResponse>>
{
    public async Task<PaginatedResponse<CustomerResponse>> Handle(
        GetCustomersQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.PageModel);

        if (request.PageModel.PageNumber <= 0 || request.PageModel.PageSize <= 0)
        {
            throw new ArgumentException(
                "PageNumber and PageSize must be greater than 0.",
                nameof(request.PageModel));
        }

        var specification = BuildSpecification(request);

        var (items, totalAmount) = await customerRepository
            .GetAllPaginatedAsync(specification, cancellationToken);

        var mappedCustomers = items.Adapt<List<CustomerResponse>>();

        return new PaginatedResponse<CustomerResponse>
        {
            Items = mappedCustomers,
            TotalAmount = totalAmount
        };
    }

    private static Specification<CustomerEntity> BuildSpecification(GetCustomersQuery request)
    {
        var builder = new SpecificationBuilder<CustomerEntity>();

        var skip = (request.PageModel.PageNumber - 1) * request.PageModel.PageSize;

        var filter = request.Model;

        if (filter is not null)
        {
            builder
                .Add<GetCustomerByEmailSpecification>(filter.Email)
                .Add<GetCustomerByNameSpecification>(filter.Name)
                .Add<GetCustomerByLoyaltyIdSpecification>(filter.LoyaltyId);
        }

        builder
            .AddInclude(x => x.Account)
            .Skip(skip)
            .Take(request.PageModel.PageSize);

        return builder.Build();
    }
}
