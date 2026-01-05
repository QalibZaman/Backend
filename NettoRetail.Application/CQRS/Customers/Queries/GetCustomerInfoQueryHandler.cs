using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Customers.Queries;

public record GetCustomerInfoQuery(string LoyaltyId) : IRequest<CustomerInfoResponse>;

public class GetCustomerInfoQueryHandler(
    ICustomerRepository customerRepository,
    TypeAdapterConfig config) : IRequestHandler<GetCustomerInfoQuery, CustomerInfoResponse>
{
    public async Task<CustomerInfoResponse> Handle(GetCustomerInfoQuery request, CancellationToken cancellationToken)
    {
        var spec = new SpecificationBuilder<CustomerEntity>()
            .Add<GetCustomerByLoyaltyIdSpecification>(request.LoyaltyId)
            .AddInclude(c => c.Account)
            .Build();

        var customer = await customerRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException("There is no customer with such loyalty id.");

        return customer.Adapt<CustomerInfoResponse>(config);
    }
}