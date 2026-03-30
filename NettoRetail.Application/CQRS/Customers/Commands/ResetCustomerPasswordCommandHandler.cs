using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Customers.Commands;

public record ResetCustomerPasswordCommand(ResetCustomerPasswordRequest model) : IRequest<Unit>;

public class ResetCustomerPasswordCommandHandler(IJwtManager jwtManager,
    ICustomerRepository customerRepository) : IRequestHandler<ResetCustomerPasswordCommand, Unit>
{
    public async Task<Unit> Handle(ResetCustomerPasswordCommand request, CancellationToken cancellationToken)
    {
        var token = jwtManager.ValidateCustomerResetPasswordToken(request.model.Token);
        var email = token.FindFirstValue(ClaimTypes.Email)
            ?? throw new BadRequestException("Reset password token does not contain an email claim.");

        var shopIdString = token.FindFirstValue(AuthClaims.ShopId)
            ?? throw new BadRequestException("Reset password token does not contain a shop ID claim.");

        if (!int.TryParse(shopIdString, out var shopId) || shopId <= 0)
            throw new BadRequestException("Shop ID in token is invalid.");

        var spec = new GetCustomerByEmailAndShopIdSpecification(email, shopId);
        var customer = await customerRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException("Customer not found.");

        var issuedAtSeconds = GetIssuedAtSeconds(token);

        var passwordUpdatedAtSeconds = new DateTimeOffset(customer.PasswordUpdatedAt).ToUnixTimeSeconds();
        if (issuedAtSeconds <= passwordUpdatedAtSeconds)
            throw new NotFoundException("Invalid token.");

        var oldhash = customer.PasswordHash;
        if (PasswordHasher.VerifyPassword(request.model.Password, oldhash))
            throw new BadRequestException("You can not use the old password");
        customer.PasswordHash = PasswordHasher.HashPassword(request.model.Password);
        customer.PasswordUpdatedAt = DateTime.UtcNow;

        await customerRepository.UpdateAsync(customer, cancellationToken);

        return Unit.Value;
    }

    private static long GetIssuedAtSeconds(ClaimsPrincipal principal)
    {
        var issuedAt = principal.FindFirst(JwtRegisteredClaimNames.Iat)?.Value;
        if (!long.TryParse(issuedAt, out var issuedAtSeconds))
            throw new NotFoundException("Invalid token.");
        return issuedAtSeconds;
    }
}
