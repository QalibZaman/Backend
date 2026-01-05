using Mapster;
using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Admin;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.IRoleService;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record CreatePlatformAdminCommand(
    CreatePlatformAdminRequest Model,
    Guid SuperAdminId) : IRequest<CreatePlatformAdminResponse>;

public class CreatePlatformAdminCommandHandler(
    IAccountRepository accountRepo,
    IUnitOfWork unitOfWork,
    IInvitationTokenRepository invitationTokenRepo,
    IInvitationTokenService invitationTokenService,
    IEmailBuilderService emailBuilderService,
    IEmailSender emailSender,
    IOptions<FrontendOptions> frontendOptions,
    IOptions<InvitationOptions> invitationOptions,
    IRoleService roleService)
    : IRequestHandler<CreatePlatformAdminCommand, CreatePlatformAdminResponse>
{
    public async Task<CreatePlatformAdminResponse> Handle(
        CreatePlatformAdminCommand request,
        CancellationToken cancellationToken)
    {
        var model = new CreatePlatformAdminModel(request.Model);

        var role = await roleService.CreatePlatformAdminRole(model, cancellationToken);

        var account = model.Adapt<AccountEntity>();

        account.Role = role;

        var rawToken = invitationTokenService.GenerateRawToken();
        var hash = invitationTokenService.Hash(rawToken);

        var expiresAt = DateTimeOffset.UtcNow.AddHours(invitationOptions.Value.PlatformAdminExpiryHours);

        var invitationToken = new InvitationTokenEntity
        {
            AccountId = account.Id,
            TokenHash = hash,
            ExpiresAt = expiresAt
        };

        using var tx = unitOfWork.BeginTransaction();


        await accountRepo.AddAsync(account, cancellationToken);

        await invitationTokenRepo.InvalidateAllActiveAsync(account.Id, cancellationToken);
        await invitationTokenRepo.AddAsync(invitationToken, cancellationToken);

        tx.Commit();

        var invitationBaseUrl = frontendOptions.Value.BaseUrl.TrimEnd('/');
        var link = $"{invitationBaseUrl}/set-password?token={rawToken}";

        var emailModel = await emailBuilderService.BuildPlatformAdminActivationEmailAsync(
            account.Email,
            account.FirstName,
            link);

        await emailSender.SendAsync(emailModel);

        return new CreatePlatformAdminResponse
        {
            AccountId = account.Id,
            Email = account.Email,
            Status = account.Status,
            Permissions = model.Permissions
        };
    }
}
