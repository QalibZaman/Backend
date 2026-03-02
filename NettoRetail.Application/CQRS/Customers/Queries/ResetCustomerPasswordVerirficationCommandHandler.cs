using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Customers.Queries;

public record ResetCustomerPasswordVerificationCommand(string Token) : IRequest<string>;
public class ResetCustomerPasswordVerirficationCommandHandler(
    IOptions<CustomerPasswordResetOptions> customerResetPasswordOptions,
    ICustomerRepository customerRepository,
    IJwtManager jwtManager) : IRequestHandler<ResetCustomerPasswordVerificationCommand, string>
{
    public async Task<string> Handle(ResetCustomerPasswordVerificationCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
            throw new BadRequestException("Activation token is required.");

        var principal = jwtManager.ValidateCustomerResetPasswordToken(command.Token);

        var email = principal.FindFirstValue(ClaimTypes.Email)
            ?? throw new BadRequestException("Reset password token does not contain an email claim.");

        var shopIdString = principal.FindFirstValue(AuthClaims.ShopId)
            ?? throw new BadRequestException("Reset password token does not contain a shop ID claim.");

        if (!int.TryParse(shopIdString, out var shopId) || shopId <= 0)
            throw new BadRequestException("Shop ID in token is invalid.");

        var spec = new GetCustomerByEmailAndShopIdSpecification(email, shopId);
        if (await customerRepository.Exists(spec, cancellationToken))
            throw new NotFoundException("Customer not found.");

        var issuedAtSeconds = GetIssuedAtSeconds(principal);
        if (account.PasswordUpdatedAt.HasValue)
        {
            var passwordUpdatedAtSeconds = new DateTimeOffset(account.PasswordUpdatedAt.Value).ToUnixTimeSeconds();
            if (issuedAtSeconds <= passwordUpdatedAtSeconds)
                throw new NotFoundException("Invalid token.");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email),
            new(AuthClaims.ShopId, shopIdString),
            new(AuthClaims.TokenType, AuthTokenTypes.CustomerResetPassword),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var resetToken = jwtManager.Generate(claims, TimeSpan.FromHours(customerResetPasswordOptions.Value.TokenExpirationHours));

        return resetToken;
    }

    private static long GetIssuedAtSeconds(ClaimsPrincipal principal)
    {
        var issuedAt = principal.FindFirst(JwtRegisteredClaimNames.Iat)?.Value;
        if (!long.TryParse(issuedAt, out var issuedAtSeconds))
            throw new NotFoundException("Invalid token.");
        return issuedAtSeconds;
    }
}
