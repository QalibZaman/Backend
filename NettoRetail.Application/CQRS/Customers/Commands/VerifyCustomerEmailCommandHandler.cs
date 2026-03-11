using System.Security.Claims;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Customers.Commands;

public record VerifyCustomerEmailCommand(string Token) : IRequest<TokenResponse>;
public class VerifyCustomerEmailCommandHandler(
    IJwtManager jwtManager,
    ICustomerRepository customerRepository)
    : IRequestHandler<VerifyCustomerEmailCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(
        VerifyCustomerEmailCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
            throw new BadRequestException("Activation token is required.");

        ClaimsPrincipal principal;

        principal = jwtManager.ValidateEmailActivation(command.Token);

        var email = principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
            throw new BadRequestException("Activation token does not contain a valid email.");

        var shopIdString = principal.FindFirstValue(AuthClaims.ShopId);
        if (string.IsNullOrWhiteSpace(shopIdString))
            throw new BadRequestException("Activation token does not contain a valid shop ID.");

        if (!int.TryParse(shopIdString, out var shopId))
            throw new BadRequestException("Shop ID in token is not a valid integer.");

        var customerSpec = new GetCustomerByEmailAndShopIdSpecification(email, shopId);
        var customer = await customerRepository.GetAsync(customerSpec, cancellationToken)
            ?? throw new NotFoundException("Customer not found.");

        if (!customer.IsActive)
        {
            customer.IsActive = true;
            await customerRepository.UpdateAsync(customer, cancellationToken);
        }

        return jwtManager.GetTokenResponse(
            customer.Id.ToString(),
            customer.Email,
            customer.RoleId.ToString(),
            customer.ShopId);
    }
}
