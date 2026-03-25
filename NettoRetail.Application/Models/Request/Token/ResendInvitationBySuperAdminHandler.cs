using MediatR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.Models.Request.Token;



public record ResendInvitationBySuperAdminCommand(Guid AccountId) : IRequest;

public class ResendInvitationBySuperAdminHandler(
    IStaffRepository accountRepo,
    IJwtManager jwtManager,
    IEmailBuilderService emailBuilderService,
    IEmailSender emailSender,
    IOptions<PlatformAdminActivationOptions> activationOptions,
    IUnitOfWork unitOfWork) : IRequestHandler<ResendInvitationBySuperAdminCommand>
{
    public async Task Handle(ResendInvitationBySuperAdminCommand request, CancellationToken cancellationToken)
    {
        var account = await accountRepo
                          .GetAsync(new GetStaffByIdSpecification(request.AccountId), cancellationToken)
                      ?? throw new NotFoundException("Account not found");

        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new(AuthClaims.TokenType, AuthTokenTypes.PlatformAdminActivation),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var rawToken = jwtManager.Generate(
            claims,
            TimeSpan.FromHours(activationOptions.Value.TokenExpirationHours));

        using var tx = unitOfWork.BeginTransaction();

        account.InvitationTokenIssuedAt = now;
        await accountRepo.UpdateAsync(account, cancellationToken);
        tx.Commit();
        
        var link = string.Format(activationOptions.Value.ActivationLink, Uri.EscapeDataString(rawToken));

        var emailModel = await emailBuilderService.BuildPlatformAdminActivationEmailAsync(account.Email, account.FirstName,link);

        await emailSender.SendAsync(emailModel, account.Id);
    }
}
