
using System.Security.Claims;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Interfaces;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Application.Services.ILinkService;

public class LinkService(IJwtManager jwtManager, IOptions<LockedAccountActivationOptions> lockedAccountActivationOptions,
    IOptions<LockedAccountActivationOptions> emailVerificationOptions) : ILinkService
{
    public string CreateBlockedAccountReactivationLink(string email, Guid accountId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email),
            new("account_id", accountId.ToString()),
            new(AuthClaims.TokenType, AuthTokenTypes.EmailVerification)
        };

        var verificationToken = jwtManager.Generate(claims, TimeSpan.FromHours(emailVerificationOptions.Value.TokenExpirationHours));

        var link = string.Format(emailVerificationOptions.Value.ActivationLink, Uri.EscapeDataString(verificationToken));

        return link;
    }
}
