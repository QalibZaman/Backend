using MapsterMapper;
using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Customers.Queries;

public record GetCustomersQuery(PaginationRequest PageModel, GetCustomersRequest? Model) : IRequest<PaginatedResponse<CustomerResponse>>;

public class GetCustomersQueryHandler(ICustomerRepository customerRepository, IMapper mapper) : IRequestHandler<GetCustomersQuery, PaginatedResponse<CustomerResponse>>
{
    public async Task<PaginatedResponse<CustomerResponse>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var specification = BuildSpecification(request);

        var (items, totalAmount) = await customerRepository.GetAllPaginatedAsync(specification, cancellationToken);

        var mappedCustomers = mapper.Map<ICollection<CustomerResponse>>(items);

        return new PaginatedResponse<CustomerResponse>()
        {
            Items = mappedCustomers,
            TotalAmount = totalAmount
        };
    }

    private Specification<CustomerEntity> BuildSpecification(GetCustomersQuery request)
    {
        var specificationBuilder = new SpecificationBuilder<CustomerEntity>();
        var skip = PaginationHelper.CalculateSkip(request.PageModel.PageNumber, request.PageModel.PageSize);

        specificationBuilder.Add<GetCustomerBySearchSpecification>(request.Model?.Search);

        specificationBuilder.Skip(skip)
            .Take(request.PageModel.PageSize);


        return specificationBuilder.Build();
    }
}
