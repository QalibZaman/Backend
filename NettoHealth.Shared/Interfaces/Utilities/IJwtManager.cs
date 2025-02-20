using System.Security.Claims;

namespace NettoHealth.Shared.Interfaces.Utilities;

public interface IJwtManager
{
    string Generate(List<Claim> claims, TimeSpan expiration);
    ClaimsPrincipal? ValidateRefreshToken(string refreshToken);
}