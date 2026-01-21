using System.Security.Claims;
using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Customers.Queries;

public record ResetCustomerPasswordVerificationCommand(string Token) : IRequest<string>;
public class ResetCustomerPasswordVerirficationCommandHandler(
    IOptions<CustomerPasswordResetOptions> customerResetPasswordOptions,
    IAccountRepository accountRepository,
    IJwtManager jwtManager) : IRequestHandler<ResetCustomerPasswordVerificationCommand, string>
{
    public async Task<string> Handle(ResetCustomerPasswordVerificationCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
            throw new BadRequestException("Activation token is required.");

        var principal = jwtManager.ValidateCustomerResetPasswordToken(command.Token);

        var emailClaim = principal.FindFirst(ClaimTypes.Email);

        var spec = new GetAccountByEmailSpecification(emailClaim!.Value);
        var account = await accountRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException("Customer not found.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, account.Email),
            new(AuthClaims.TokenType, AuthTokenTypes.CustomerResetPassword)
        };

        var resetToken = jwtManager.Generate(claims, TimeSpan.FromHours(customerResetPasswordOptions.Value.TokenExpirationHours));

        return resetToken;
    }
}
