using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Customer;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Customers.Queries;

public record GetCustomerContactInfoQuery(Guid AccountId, int shopId) : IRequest<CustomerContactInfoResponse>;

public class GetCustomerContactInfoQueryHandler(
    ICustomerRepository customerRepository,
    TypeAdapterConfig config)
    : IRequestHandler<GetCustomerContactInfoQuery, CustomerContactInfoResponse>
{
    public async Task<CustomerContactInfoResponse> Handle(GetCustomerContactInfoQuery request, CancellationToken ct)
    {
        if (request.AccountId == Guid.Empty)
            throw new ArgumentException("AccountId must not be empty.", nameof(request.AccountId));

        var spec = new GetCustomerByIdAndShopIdSpecification(request.AccountId, request.shopId);

        var customer = await customerRepository.GetAsync(spec, ct)
                       ?? throw new NotFoundException("Customer not found.");

        if (customer.Account is null)
            throw new InvalidOperationException("Customer contact information is unavailable.");

        if (string.IsNullOrWhiteSpace(customer.Account.FirstName) ||
            string.IsNullOrWhiteSpace(customer.Account.LastName))
            throw new InvalidOperationException("Customer contact information is incomplete.");

        return customer.Adapt<CustomerContactInfoResponse>(config);
    }
}