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

public class GetCustomerInfoQueryHandler(ICustomerRepository customerRepository) : IRequestHandler<GetCustomerInfoQuery, CustomerInfoResponse>
{
    public async Task<CustomerInfoResponse> Handle(GetCustomerInfoQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<CustomerEntity>();
        specificationBuilder.Add<GetCustomerByLoyaltyIdSpecification>(request.LoyaltyId)
            .AddInclude(c => c.Account);

        var customer = await customerRepository.GetAsync(specificationBuilder.Build(), cancellationToken) ?? throw new NotFoundException("There is no customer with such loyalty id.");

        var response = customer.Adapt<CustomerInfoResponse>();

        return response;
    }
}
