using System.Security.Claims;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Interfaces;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Application.Services.ILinkService;

public class LinkService(IJwtManager jwtManager, IOptions<LockedAccountActivationOptions> lockedAccountActivationOptions) : ILinkService
{
    public string CreateBlockedAccountReactivationLink(string email, Guid accountId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email),
            new(AuthClaims.AccountId, accountId.ToString()),
            new(AuthClaims.TokenType, AuthTokenTypes.LockedAccountActivation)
        };
        var verificationToken = jwtManager.Generate(claims, TimeSpan.FromHours(lockedAccountActivationOptions.Value.TokenExpirationHours));
        var link = string.Format(lockedAccountActivationOptions.Value.ActivationLink, Uri.EscapeDataString(verificationToken));

        return link;
    }
}
