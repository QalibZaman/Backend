using System.Security.Claims;

namespace NettoRetail.Shared.Interfaces.Utilities;

public interface IJwtManager
{
    string Generate(List<Claim> claims, TimeSpan expiration);
    ClaimsPrincipal? ValidateToken(string refreshToken);
}