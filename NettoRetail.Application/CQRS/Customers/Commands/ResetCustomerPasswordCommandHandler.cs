using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Customers.Commands;

public record ResetCustomerPasswordCommand(ResetCustomerPasswordRequest model) : IRequest<Unit>;

public class ResetCustomerPasswordCommandHandler(IJwtManager jwtManager,
    IAccountRepository accountRepository) : IRequestHandler<ResetCustomerPasswordCommand, Unit>
{
    public async Task<Unit> Handle(ResetCustomerPasswordCommand request, CancellationToken cancellationToken)
    {
        var token = jwtManager.ValidateCustomerResetPasswordToken(request.model.Token);
        var emailClaim = token.FindFirst(ClaimTypes.Email)
            ?? throw new BadRequestException("Reset password token does not contain an email claim.");

        var spec = new GetAccountByEmailSpecification(emailClaim!.Value);
        var account = await accountRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException("Customer not found.");

        var issuedAtSeconds = GetIssuedAtSeconds(token);
        if (account.PasswordUpdatedAt.HasValue)
        {
            var passwordUpdatedAtSeconds = new DateTimeOffset(account.PasswordUpdatedAt.Value).ToUnixTimeSeconds();
            if (issuedAtSeconds <= passwordUpdatedAtSeconds)
                throw new NotFoundException("Invalid token.");
        }

        var oldhash = account.PasswordHash;
        if (PasswordHasher.VerifyPassword(request.model.Password, oldhash))
            throw new BadRequestException("You can not use the old password");
        account.PasswordHash = PasswordHasher.HashPassword(request.model.Password);
        account.PasswordUpdatedAt = DateTime.UtcNow;

        await accountRepository.UpdateAsync(account, cancellationToken);

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
