using System.Security.Claims;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Interfaces;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Application.Services.ILinkService;

public class LinkService(IJwtManager jwtManager,
    IOptions<EmailVerificationOptions> emailVerificationOptions) : ILinkService
{
    // Creates a verification link for customer account activation in swagger.
    public Task<string> CreateCustomerAccountVerificationLink(string email)
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
}
