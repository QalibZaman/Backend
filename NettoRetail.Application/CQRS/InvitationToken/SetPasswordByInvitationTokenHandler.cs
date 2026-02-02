using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Password;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.InvitationToken;
using NettoRetail.Domain.Enums;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.InvitationToken;

public record SetPasswordByInvitationTokenCommand(SetPasswordByInvitationRequest Model) : IRequest<TokenResponse>;

public class SetPasswordByInvitationTokenHandler(
    IInvitationTokenRepository invitationTokenRepository,
    IAccountRepository accountRepository,
    IInvitationTokenService invitationTokenService,
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

        var now = DateTime.UtcNow;

        var tokenHash = invitationTokenService.Hash(model.Token);

        var token = await invitationTokenRepository.GetByHashAsync(
            new GetInvitationTokenByHashSpecification(tokenHash),
            cancellationToken);

        if (token is null || token.UsedAt is not null || token.InvalidatedAt is not null || token.ExpiresAt <= now)
        {
            throw new ConflictException("Your invitation link is invalid or expired.");
        }

        var account = await accountRepository.GetAsync(
            new GetAccountByIdSpecification(token.AccountId),
            cancellationToken) 
                      ?? throw new NotFoundException("This account no longer exists. Contact your administrator.");

        if (account.Status == StaffStatusEnum.Active)
            throw new ConflictException("Account is already activated.");
        if (account.Status != StaffStatusEnum.Pending)
        {
            throw new ConflictException("Account cannot be activated.");
        }

        token.UsedAt = now;

        account.PasswordHash = PasswordHasher.HashPassword(model.Password);
        account.PasswordUpdatedAt = now;
        account.Status = StaffStatusEnum.Active;

        await invitationTokenRepository.UpdateAsync(token, cancellationToken);
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
}
