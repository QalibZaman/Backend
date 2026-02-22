using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Options;

namespace NettoRetail.Application.Services.ILinkService;

public class LinkService(IJwtManager jwtManager,
    IOptions<EmailVerificationOptions> emailVerificationOptions,
    IOptions<CustomerPasswordResetOptions> customerResetPasswordOptions,
    IOptions<NewsletterOptions> newsletterOptions) : ILinkService
{
    // Creates a verification link for customer account activation in swagger.
    public string CreateCustomerAccountVerificationLink(string email)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email),
            new(AuthClaims.TokenType, AuthTokenTypes.EmailVerification)
        };

        var verificationToken = jwtManager.Generate(claims, TimeSpan.FromHours(emailVerificationOptions.Value.TokenExpirationHours));

        var link = string.Format(emailVerificationOptions.Value.VerificationLink, Uri.EscapeDataString(verificationToken));

        return link;
    }

    public string CreateCustomerResetPasswordLink(string email)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email),
            new(AuthClaims.TokenType, AuthTokenTypes.CustomerResetPassword),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var resetToken = jwtManager.Generate(claims, TimeSpan.FromHours(customerResetPasswordOptions.Value.TokenExpirationHours));

        var link = string.Format(customerResetPasswordOptions.Value.ResetPasswordLink, Uri.EscapeDataString(resetToken));

        return link;
    }

    public string BuildNewsletterUnsubscribeUrl(Guid accountId)
    {
        var claims = new List<Claim>
        {
            new("account_id", accountId.ToString()),
            new(AuthClaims.TokenType, AuthTokenTypes.NewsletterUnsubscribe)
        };
// Newsletter unsubscribe links are always valid according to IM-5
        var token = jwtManager.GenerateWihoutTime(claims);
        if (string.IsNullOrWhiteSpace(newsletterOptions.Value.NewsletterLink))
            throw new BadRequestException("NewsletterOptions:NewsletterLink missing/empty.");
        var link= string.Format(newsletterOptions.Value.NewsletterLink, Uri.EscapeDataString(token));

        return link;
    }
}
