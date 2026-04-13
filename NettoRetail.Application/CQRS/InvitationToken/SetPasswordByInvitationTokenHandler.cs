using MediatR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Password;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Enums;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.InvitationToken;

public record SetPasswordByInvitationTokenCommand(SetPasswordByInvitationRequest Model) : IRequest<TokenResponse>;

public class SetPasswordByInvitationTokenHandler(
    IStaffRepository accountRepository,
    IJwtManager jwtManager,
    IEmailSender emailSender,
    IEmailBuilderService emailBuilderService
) : IRequestHandler<SetPasswordByInvitationTokenCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(SetPasswordByInvitationTokenCommand request, CancellationToken cancellationToken)
    {
        var model = request.Model;

        if (string.IsNullOrWhiteSpace(model.Token))
        {
            throw new BadRequestException("Your invitation link is invalid or expired.");
        }

        ClaimsPrincipal principal;
        try
        {
            principal = jwtManager.ValidatePlatformAdminResetPasswordToken(model.Token);
        }
        catch (BadRequestException)
        {
            throw new ConflictException("Your invitation link is invalid or expired.");
        }

        var accountId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(accountId))
        {
            throw new ConflictException("Your invitation link is invalid or expired.");
        }

        var account = await accountRepository.GetAsync(
            new GetStaffByIdSpecification(Guid.Parse(accountId)),
            cancellationToken) 
                      ?? throw new NotFoundException("This account no longer exists. Contact your administrator.");

        if (account.Status == StaffStatusEnum.Active)
            throw new ConflictException("Account is already activated.");
        if (account.Status != StaffStatusEnum.Pending)
        {
            throw new ConflictException("Account cannot be activated.");
        }

        var issuedAtSeconds = GetIssuedAtSeconds(principal);
        if (account.InvitationTokenIssuedAt.HasValue)
        {
            var latestIssuedAtSeconds = new DateTimeOffset(account.InvitationTokenIssuedAt.Value).ToUnixTimeSeconds();
            if (issuedAtSeconds < latestIssuedAtSeconds)
                throw new ConflictException("Your invitation link is invalid or expired.");
        }

        if (account.PasswordUpdatedAt != default)
        {
            var passwordUpdatedAtSeconds = new DateTimeOffset(account.PasswordUpdatedAt).ToUnixTimeSeconds();
            if (issuedAtSeconds <= passwordUpdatedAtSeconds)
                throw new ConflictException("Your invitation link is invalid or expired.");
        }

        var now = DateTime.UtcNow;
        account.PasswordHash = PasswordHasher.HashPassword(model.Password);
        account.PasswordUpdatedAt = now;
        account.Status = StaffStatusEnum.Active;

        await accountRepository.UpdateAsync(account, cancellationToken);

        var tokenResponse = jwtManager.GetTokenResponse(
            account.Id.ToString(),
            account.Email,
            account.RoleId.ToString(),
            account.ShopId);
        var confirmationEmail = await emailBuilderService.BuildPlatformAdminActivatedEmailAsync(
            account.Email,
            account.FirstName);

        await emailSender.SendAsync(confirmationEmail, account.Id);

        return tokenResponse;
    }

    private static long GetIssuedAtSeconds(ClaimsPrincipal principal)
    {
        var issuedAt = principal.FindFirst(JwtRegisteredClaimNames.Iat)?.Value;
        if (!long.TryParse(issuedAt, out var issuedAtSeconds))
            throw new NotFoundException("Invalid token.");
        return issuedAtSeconds;
    }
}
