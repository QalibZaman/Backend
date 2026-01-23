using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Customer;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Customers.Queries;

public record GetCustomerContactInfoQuery(Guid AccountId,int shopId) : IRequest<CustomerContactInfoResponse>;

public class GetCustomerContactInfoQueryHandler(
    ICustomerRepository customerRepository,
    TypeAdapterConfig config) : IRequestHandler<GetCustomerContactInfoQuery, CustomerContactInfoResponse>
{
    public async Task<CustomerContactInfoResponse> Handle(GetCustomerContactInfoQuery request, CancellationToken ct)
    {
        var spec = new GetCustomerByIdAndShopIdSpecification(request.AccountId, request.shopId);

        var customer = await customerRepository.GetAsync(spec, ct)
                       ?? throw new NotFoundException("Customer not found.");
        if (customer.AccountId == Guid.Empty) throw new Exception("The id of account inside the customer is null");
       
        if (customer.Account is null) throw new BadRequestException("The account info of the customer is empty");
        var firstName = customer.Account.FirstName;
        var lastName = customer.Account.LastName;

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new Exception("The name and Surname is reqired");

        var response = customer.Adapt<CustomerContactInfoResponse>();
        return response;
    }
}
