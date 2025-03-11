using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Application.Interfaces;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.API.Infrastructure.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection ConfigureAuthentication(this IServiceCollection services)
    {
        // When we'll be introducing the Google authentication,
        // we will be adding another scheme with specific options for it.

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer("Default", options =>
            {
                var jwtOptions = services.BuildServiceProvider()
                    .GetRequiredService<IOptions<JwtOptions>>()
                    .Value;

                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var tokenTypeClaim = context.Principal?.FindFirst("token_type")?.Value;
                        if (tokenTypeClaim != "access")
                        {
                            context.Fail("Invalid token type. Expected an access token.");
                            return;
                        }

                        var blacklistTokenService = context.HttpContext.RequestServices
                                                   .GetRequiredService<IBlacklistTokenService>();

                        var token = JwtUtils.ExtractTokenFromAuthorizationHeader(context.HttpContext);

                        if (!string.IsNullOrEmpty(token))
                        {
                            bool isBlacklisted = await blacklistTokenService.IsTokenBlacklistedAsync(
                                token, context.HttpContext.RequestAborted);

                            if (isBlacklisted)
                            {
                                context.Fail("Token is invalid or has been revoked.");
                            }
                        }
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes("Default")
                .Build();

            // We can later on add the Google policy using options.AddPolicy()
            // note: you should obviously have a policy pre-defined in AddAuthentication before creating an authorization policy.
        });
        return services;
    }
}