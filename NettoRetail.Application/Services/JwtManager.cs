using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Options;

namespace NettoRetail.Application.Services;

public class JwtManager(IOptions<JwtOptions> jwtOptions) : IJwtManager
{
    private readonly byte[] _key = Encoding.UTF8.GetBytes(jwtOptions.Value.SecretKey);
    private readonly string _algorithm = SecurityAlgorithms.HmacSha512;

    public string Generate(List<Claim> claims, TimeSpan expiration)
    {
        var handler = new JwtSecurityTokenHandler();

        var claimsIdentity = new ClaimsIdentity(claims);

        var securityKey = new SymmetricSecurityKey(_key);
        var signingCredentials = new SigningCredentials(securityKey, _algorithm);

        var expires = DateTime.UtcNow.Add(expiration);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = claimsIdentity,
            Expires = expires,
            SigningCredentials = signingCredentials,
        };

        var token = handler.CreateToken(tokenDescriptor);

        return handler.WriteToken(token);
    }

    public ClaimsPrincipal? Validate(string refreshToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(
            refreshToken,
            new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(_key)
            },
            out _);

        return principal;
    }

    public TokenResponse GetTokenResponse(string userId, string userEmail, string userRoleId)
    {
        const string tokenType = "token_type";

        var accessTokenExpirationMinutes = jwtOptions.Value.AccessTokenExpirationMinutes;
        var refreshTokenExpirationDays = jwtOptions.Value.RefreshTokenExpirationDays;

        var accessTokenClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, userEmail),
            new(ClaimTypes.Role, userRoleId),
            new(tokenType, "access")
        };
        var refreshTokenClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(tokenType, "refresh")
        };

        var accessToken = Generate(accessTokenClaims, TimeSpan.FromMinutes(accessTokenExpirationMinutes));
        var refreshToken = Generate(refreshTokenClaims, TimeSpan.FromDays(refreshTokenExpirationDays));

        var tokenResponse = new TokenResponse(accessToken, refreshToken, null);

        return tokenResponse;
    }

    public string GenerateTempToken(string userId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new("token_type", "temp")
        };

        return Generate(claims, TimeSpan.FromMinutes(jwtOptions.Value.TempTokenExpirationMinutes));
    }

}