using System.Security.Claims;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Interfaces;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Application.Services.ILinkService;

public class LinkService(IJwtManager jwtManager,
    IOptions<EmailVerificationOptions> emailVerificationOptions) : ILinkService
{
    // That method was created to give link for customer activation in respone for swagger testing
    public Task<string> CreateCustomerAccountVerificationLink(string email)
    {
        EmailVerificationOptions options = new();

        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, email),
            new(AuthClaims.TokenType, AuthTokenTypes.EmailVerification)
        };

        var verificationToken = jwtManager.Generate(claims, TimeSpan.FromHours(emailVerificationOptions.Value.TokenExpirationHours));

        var link = string.Format(emailVerificationOptions.Value.VerificationLink, Uri.EscapeDataString(verificationToken));

        return Task.FromResult(link);
    }
}
