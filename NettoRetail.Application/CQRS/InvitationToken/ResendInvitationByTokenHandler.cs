using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Token;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.InvitationToken;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.InvitationToken;

public record ResendInvitationByTokenCommand(string Token):IRequest
{
}

public class ResendInvitationByTokenHandler(
    IInvitationTokenRepository invitationTokenRepo,
    IAccountRepository accountRepo,
    IInvitationTokenService invitationTokenService,
    IEmailBuilderService emailBuilderService,
    IEmailSender emailSender,
    IOptions<FrontendOptions> frontendOptions,
    IOptions<InvitationOptions> invitationOptions,
    IUnitOfWork unitOfWork
    ):IRequestHandler<ResendInvitationByTokenCommand>
{
    public async Task Handle(ResendInvitationByTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            throw new ConflictException("Your invitation link is invalid or expired.");
        var oldhash = invitationTokenService.Hash(request.Token);
        
        var oldtoken = await invitationTokenRepo
            .GetByHashAsync(new GetInvitationTokenByHashSpecification(oldhash), cancellationToken)
            ?? throw new ConflictException("Your invitation link is invalid or expired.");
        
        var account = await accountRepo
            .GetAsync(new GetAccountByIdSpecification(oldtoken.AccountId),cancellationToken)
                      ?? throw new NotFoundException("This account no longer exists. Contact your administrator.");

        var rawToken = invitationTokenService.GenerateRawToken();
        var rawHash = invitationTokenService.Hash(rawToken);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(invitationOptions.Value.PlatformAdminExpiryHours);

        var newtoken = new InvitationTokenEntity
        {
            AccountId = account.Id,
            TokenHash = rawHash,
            ExpiresAt = expiresAt
        };

        using var tx = unitOfWork.BeginTransaction();

        await invitationTokenRepo.InvalidateAllActiveAsync(newtoken.AccountId, cancellationToken);
        await invitationTokenRepo.UpdateAsync(newtoken, cancellationToken);
        
        tx.Commit();

        var url = frontendOptions.Value.BaseUrl;
        var link = $"{url}/set-password?token={rawToken}";

        var emailModel = await emailBuilderService.BuildPlatformAdminActivationEmailAsync(account.Email, account.FirstName,link);

        await emailSender.SendAsync(emailModel);
    }
}