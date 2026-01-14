using System.Security.Claims;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Customers.Commands;

public record VerifyCustomerEmailCommand(string Token) : IRequest<Unit>;
public class VerifyCustomerEmailCommandHandler(
    IJwtManager jwtManager,
    ICustomerRepository customerRepository)
    : IRequestHandler<VerifyCustomerEmailCommand, Unit>
{
    public async Task<Unit> Handle(
        VerifyCustomerEmailCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
            throw new BadRequestException("Activation token is required.");

        ClaimsPrincipal principal;

        principal = jwtManager.ValidateEmailActivation(command.Token);

        var emailClaim = principal.FindFirst(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(emailClaim?.Value))
            throw new BadRequestException("Activation token does not contain a valid email.");
        var email = emailClaim.Value;

        var customerSpec = new GetCustomerByEmailSpecification(email);
        var customer = await customerRepository.GetAsync(customerSpec, cancellationToken)
            ?? throw new NotFoundException("Customer not found.");

        if (!customer.IsActive)
        {
            customer.IsActive = true;
            await customerRepository.UpdateAsync(customer, cancellationToken);
        }

        return Unit.Value;
    }
}
