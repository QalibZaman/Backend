using System.Security.Claims;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Interfaces;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Application.Services;

public class LinkService(IJwtManager jwtManager, IOptions<NewsletterOptions> newsletterOptions) : ILinkService
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
}