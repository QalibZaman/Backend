using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Constants;
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

    public string GenerateWihoutTime(List<Claim> claims)
    {
        if (_key is null || _key.Length == 0)
            throw new BadRequestException("JwtOptions.SecretKey is empty. JWT generation cannot work.");

        var handler = new JwtSecurityTokenHandler();

        var claimsIdentity = new ClaimsIdentity(claims);

        var securityKey = new SymmetricSecurityKey(_key);
        var signingCredentials = new SigningCredentials(securityKey, _algorithm);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = claimsIdentity,
            SigningCredentials = signingCredentials,
        };

        var token = handler.CreateToken(tokenDescriptor);

        var jwt = handler.WriteToken(token);

        if (string.IsNullOrWhiteSpace(jwt))
            throw new BadRequestException("JWT generation returned empty string.");

        return jwt;
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

    public ClaimsPrincipal ValidateResetToken(string resetToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(
            resetToken,
            new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(_key),
            },
            out _);

        var tokenType = principal.FindFirst("token_type")?.Value;

        if (tokenType != "reset")
        {
            throw new BadRequestException("Invalid token type.");
        }

        return principal;
    }

    public ClaimsPrincipal ValidateEmailActivation(string emailActivationToken)
    {
        var handler = new JwtSecurityTokenHandler();

        try
        {
            var principal = handler.ValidateToken(
            emailActivationToken,
            new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_key),
                ClockSkew = TimeSpan.Zero
            },
            out _);

            var tokenType = principal.FindFirst(AuthClaims.TokenType)?.Value;

            if (tokenType != AuthTokenTypes.EmailVerification)
            {
                throw new BadRequestException("Invalid token type.");
            }

            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                throw new BadRequestException("Email claim is missing in token.");

            return principal;
        }
        catch (SecurityTokenExpiredException)
        {
            throw new BadRequestException("Token has expired.");
        }
        catch (SecurityTokenMalformedException)
        {
            throw new BadRequestException("Token format is invalid.");
        }
        catch (SecurityTokenException)
        {
            throw new BadRequestException("Invalid token.");
        }
    }

    public ClaimsPrincipal ValidatePlatformAdminActivationToken(string activationToken)
        => ValidatePlatformAdminTokenInternal(activationToken, AuthTokenTypes.PlatformAdminActivation, allowExpired: false);

    public ClaimsPrincipal ValidatePlatformAdminActivationTokenAllowExpired(string activationToken)
        => ValidatePlatformAdminTokenInternal(activationToken, AuthTokenTypes.PlatformAdminActivation, allowExpired: true);

    public ClaimsPrincipal ValidatePlatformAdminResetPasswordToken(string resetToken)
        => ValidatePlatformAdminTokenInternal(resetToken, AuthTokenTypes.PlatformAdminResetPassword, allowExpired: false);

    private ClaimsPrincipal ValidatePlatformAdminTokenInternal(string token, string expectedType, bool allowExpired)
    {
        var handler = new JwtSecurityTokenHandler();

        try
        {
            var principal = handler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = !allowExpired,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(_key),
                    ClockSkew = TimeSpan.Zero
                },
                out _);

            var tokenType = principal.FindFirst(AuthClaims.TokenType)?.Value;
            if (tokenType != expectedType)
            {
                throw new BadRequestException("Invalid token type.");
            }

            var accountId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountId))
                throw new BadRequestException("Account id claim is missing in token.");

            var issuedAt = principal.FindFirst(JwtRegisteredClaimNames.Iat)?.Value;
            if (string.IsNullOrEmpty(issuedAt))
                throw new BadRequestException("Token issued-at claim is missing in token.");

            return principal;
        }
        catch (SecurityTokenExpiredException)
        {
            throw new BadRequestException("Token has expired.");
        }
        catch (SecurityTokenMalformedException)
        {
            throw new BadRequestException("Token format is invalid.");
        }
        catch (SecurityTokenException)
        {
            throw new BadRequestException("Invalid token.");
        }
    }

    public TokenResponse GetTokenResponse(string userId, string userEmail, string userRoleId, int? shopId = null)
    {
        const string tokenType = AuthClaims.TokenType;

        var accessTokenExpirationMinutes = jwtOptions.Value.AccessTokenExpirationMinutes;
        var refreshTokenExpirationDays = jwtOptions.Value.RefreshTokenExpirationDays;

        var accessTokenClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, userEmail),
            new(ClaimTypes.Role, userRoleId),
            new(tokenType, AuthTokenTypes.Access)
        };

        if (shopId != null)
        {
            accessTokenClaims.Add(new Claim("shop_id", shopId.Value.ToString()));
        }

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
            new(AuthClaims.TokenType, AuthTokenTypes.Temp)
        };

        return Generate(claims, TimeSpan.FromMinutes(jwtOptions.Value.TempTokenExpirationMinutes));
    }

    public ClaimsPrincipal ValidateCustomerResetPasswordToken(string customerResetPasswordToken)
    {
        var handler = new JwtSecurityTokenHandler();

        try
        {
            var principal = handler.ValidateToken(
            customerResetPasswordToken,
            new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_key),
                ClockSkew = TimeSpan.Zero
            },
            out _);

            var tokenType = principal.FindFirst(AuthClaims.TokenType)?.Value;

            if (tokenType != AuthTokenTypes.CustomerResetPassword)
            {
                throw new BadRequestException("Invalid token type.");
            }

            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                throw new BadRequestException("Email claim is missing in token.");

            return principal;
        }
        catch (SecurityTokenExpiredException)
        {
            throw new BadRequestException("Token has expired.");
        }
        catch (SecurityTokenMalformedException)
        {
            throw new BadRequestException("Token format is invalid.");
        }
        catch (SecurityTokenException)
        {
            throw new BadRequestException("Invalid token.");
        }
        catch (ArgumentException)
        {
            throw new NotFoundException("Invalid token.");
        }
        catch (FormatException)
        {
            throw new NotFoundException("Invalid token.");
        }
    }

    public ClaimsPrincipal ValidateChangeEmailToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(jwtOptions.Value.SecretKey);

        try
        {
            var principal = tokenHandler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                },
                out _);

            var tokenType = principal.FindFirst(AuthClaims.TokenType)?.Value;

            if (tokenType != AuthTokenTypes.CustomerEmailChange)
            {
                throw new BadRequestException("Invalid token type.");
            }

            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                throw new BadRequestException("Email claim is missing in token.");

            return principal;
        }
        catch (SecurityTokenExpiredException)
        {
            throw new BadRequestException("Token has expired.");
        }
        catch (SecurityTokenMalformedException)
        {
            throw new BadRequestException("Token format is invalid.");
        }
        catch (SecurityTokenException)
        {
            throw new BadRequestException("Invalid token.");
        }
        catch (ArgumentException)
        {
            throw new NotFoundException("Invalid token.");
        }
        catch (FormatException)
        {
            throw new NotFoundException("Invalid token.");
        }
    }
}