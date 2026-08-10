using System.Security.Claims;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Interfaces;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Application.Services;

public class LinkService(IJwtManager jwtManager, IOptions<NewsletterOptions> newsletterOptions, IOptions<LockedAccountActivationOptions> lockedAccountActivationOptions) : ILinkService
{
    public string BuildNewsletterUnsubscribeUrl(Guid accountId)
    {
        var claims = new List<Claim>
        {
            new(AuthClaims.ShopId, accountId.ToString()),
            new(AuthClaims.TokenType, AuthTokenTypes.NewsletterUnsubscribe)
        };
        var token = jwtManager.GenerateWihoutTime(claims);
        var link = string.Format(newsletterOptions.Value.NewsletterLink, Uri.EscapeDataString(token));
        return link;
    }
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
