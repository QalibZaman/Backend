using System.Security.Claims;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Customers.Commands;

public record VerifyCustomerEmailCommand(string Token) : IRequest;
public class VerifyCustomerEmailCommandHandler(
    IJwtManager jwtManager,
    ICustomerRepository customerRepository,
    IAccountRepository accountRepository)
    : IRequestHandler<VerifyCustomerEmailCommand>
{
    public async Task Handle(
        VerifyCustomerEmailCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
            throw new BadRequestException("Activation token is required.");

        ClaimsPrincipal principal;

        try
        {
            principal = jwtManager.ValidateEmailActivation(command.Token);
        }
        catch (SecurityTokenExpiredException)
        {
            throw new MyUnauthorizedException("The activation link has expired.");
        }
        catch (SecurityTokenException)
        {
            throw new BadRequestException("Invalid activation token.");
        }

        var email = principal.FindFirst(ClaimTypes.Email)?.Value;

        var accSpec = new GetAccountByEmailSpecification(email!);
        var account = await accountRepository.GetAsync(accSpec, cancellationToken)
            ?? throw new BadRequestException("Account not found.");

        var customerSpec = new GetCustomerByIdSpecification(account.Id);
        var customer = await customerRepository.GetAsync(customerSpec, cancellationToken)
            ?? throw new BadRequestException("Customer not found.");

        if (!customer.IsActive)
        {
            customer.IsActive = true;
            await customerRepository.UpdateAsync(customer, cancellationToken);
        }
    }
}
