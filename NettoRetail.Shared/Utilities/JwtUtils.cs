using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;

namespace NettoRetail.Shared.Utilities;

public static class JwtUtils
{
    public static bool IsJwtExpired(string jwtToken)
    {
        if (string.IsNullOrEmpty(jwtToken)) return true;

        var handler = new JwtSecurityTokenHandler();
        try
        {
            var token = handler.ReadJwtToken(jwtToken);
            var expClaim = token?.Claims.FirstOrDefault(c => c.Type == "exp");
            if (expClaim == null) return true;

            var expirationDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim.Value)).DateTime;
            return expirationDate < DateTime.UtcNow;
        }
        catch
        {
            return true;
        }
    }

    public static string? ExtractTokenFromAuthorizationHeader(HttpContext httpContext)
    {
        var authorizationHeader = httpContext.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            return null;
        }

        return authorizationHeader.Replace("Bearer ", "");
    }
}
