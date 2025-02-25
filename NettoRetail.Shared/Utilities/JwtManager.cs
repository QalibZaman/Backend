using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Options;

namespace NettoRetail.Shared.Utilities;

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

    public ClaimsPrincipal? ValidateToken(string refreshToken)
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
}