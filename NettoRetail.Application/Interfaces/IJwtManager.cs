using System.Security.Claims;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.Application.Interfaces;

public interface IJwtManager
{
    string Generate(List<Claim> claims, TimeSpan expiration);
    ClaimsPrincipal? Validate(string refreshToken);
    string GenerateTempToken(string userId);

    TokenResponse GetTokenResponse(string userId, string userEmail, string userRoleId, int? shopId);
}