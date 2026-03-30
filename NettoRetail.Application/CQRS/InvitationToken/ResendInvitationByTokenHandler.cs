using MediatR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Token;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.InvitationToken;

public record ResendInvitationByTokenCommand(string Token):IRequest
{
}

public class ResendInvitationByTokenHandler(
    IStaffRepository accountRepo,
    IJwtManager jwtManager,
    IEmailBuilderService emailBuilderService,
    IEmailSender emailSender,
    IOptions<PlatformAdminActivationOptions> activationOptions,
    IUnitOfWork unitOfWork
    ):IRequestHandler<ResendInvitationByTokenCommand>
{
    public async Task Handle(ResendInvitationByTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            throw new ConflictException("Your invitation link is invalid or expired.");

        ClaimsPrincipal principal;
        try
        {
            principal = jwtManager.ValidatePlatformAdminActivationTokenAllowExpired(request.Token);
        }
        catch (BadRequestException)
        {
            throw new ConflictException("Your invitation link is invalid or expired.");
        }

        var accountId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(accountId))
            throw new ConflictException("Your invitation link is invalid or expired.");
        
        var account = await accountRepo
            .GetAsync(new GetStaffByIdSpecification(Guid.Parse(accountId)),cancellationToken)
                      ?? throw new NotFoundException("This account no longer exists. Contact your administrator.");

        if (account.Status == Domain.Enums.StaffStatusEnum.Active)
            throw new ConflictException("Account is already activated.");

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
