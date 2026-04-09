using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace NettoRetail.Application.CQRS.InvitationToken;

public record ValidateInvitationTokenCommand(string Token) : IRequest<string>;

public class ValidateInvitationTokenHandler(
    IStaffRepository accountRepository,
    IJwtManager jwtManager,
    IOptions<PlatformAdminActivationOptions> activationOptions) : IRequestHandler<ValidateInvitationTokenCommand, string>
{
    public async Task<string> Handle(ValidateInvitationTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            throw new BadRequestException("Invitation token is empty");
        }

        ClaimsPrincipal principal;
        try
        {
            principal = jwtManager.ValidatePlatformAdminActivationToken(request.Token);
        }
        catch (BadRequestException)
        {
            throw new ConflictException("Your invitation link is invalid or expired.");
        }

        var accountId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new ConflictException("Your invitation link is invalid or expired.");

        var issuedAtSeconds = GetIssuedAtSeconds(principal);

        var account = await accountRepository.GetAsync(
            new GetStaffByIdSpecification(Guid.Parse(accountId)),
            cancellationToken) ?? throw new NotFoundException("This account no longer exists. Contact your administrator.");

        if (account.Status != Domain.Enums.StaffStatusEnum.Pending)
            throw new ConflictException("Account cannot be activated.");

        if (account.InvitationTokenIssuedAt.HasValue)
        {
            var latestIssuedAtSeconds = new DateTimeOffset(account.InvitationTokenIssuedAt.Value).ToUnixTimeSeconds();
            if (issuedAtSeconds < latestIssuedAtSeconds)
                throw new ConflictException("Your invitation link is invalid or expired.");
        }

        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new(AuthClaims.TokenType, AuthTokenTypes.PlatformAdminResetPassword),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var resetToken = jwtManager.Generate(
            claims,
            TimeSpan.FromHours(activationOptions.Value.TokenExpirationHours));

        account.InvitationTokenIssuedAt = now;
        await accountRepository.UpdateAsync(account, cancellationToken);

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
