using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.Models.Request.Token;



public record ResendInvitationBySuperAdminCommand(Guid AccountId) : IRequest;

public class ResendInvitationBySuperAdminHandler(
    IInvitationTokenRepository invitationTokenRepo,
    IAccountRepository accountRepo,
    IInvitationTokenService invitationTokenService,
    IEmailBuilderService emailBuilderService,
    IEmailSender emailSender,
    IOptions<FrontendOptions> frontendOptions,
    IOptions<InvitationOptions> invitationOptions,
    IUnitOfWork unitOfWork) : IRequestHandler<ResendInvitationBySuperAdminCommand>
{
    public async Task Handle(ResendInvitationBySuperAdminCommand request, CancellationToken cancellationToken)
    {
        var account = await accountRepo
                          .GetAsync(new GetAccountByIdSpecification(request.AccountId), cancellationToken)
                      ?? throw new NotFoundException("Account not found");

        var rawToken = invitationTokenService.GenerateRawToken();
        var rawhash = invitationTokenService.Hash(rawToken);
        var expiredAt = DateTimeOffset.UtcNow.AddHours(invitationOptions.Value.PlatformAdminExpiryHours);

        var token = new InvitationTokenEntity
        {
            AccountId = request.AccountId,
            TokenHash = rawhash,
            ExpiresAt = expiredAt
        };

        using var tx = unitOfWork.BeginTransaction();

        await invitationTokenRepo.InvalidateAllActiveAsync(request.AccountId, cancellationToken);
        await invitationTokenRepo.UpdateAsync(token, cancellationToken);
        tx.Commit();
        
        var url = frontendOptions.Value.BaseUrl;
        var link = $"{url}/set-password?token={rawToken}";

        var emailModel = await emailBuilderService.BuildPlatformAdminActivationEmailAsync(account.Email, account.FirstName,link);

        await emailSender.SendAsync(emailModel, account.Id);
    }
}