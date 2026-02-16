using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NettoRetail.Application.Interfaces;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Options;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.API.Infrastructure.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection ConfigureAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(AuthSchemes.Default, options =>
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
                    OnTokenValidated = context =>
                    {
                        var tokenTypeClaim = context.Principal?.FindFirst(AuthClaims.TokenType)?.Value;
                        var requestPath = context.HttpContext.Request.Path.Value;

                        if (tokenTypeClaim != AuthTokenTypes.Access)
                        {
                            context.Fail("Invalid token type. Expected an access token.");
                            return Task.CompletedTask;
                        }

                        return Task.CompletedTask;
                    }
                };
            })
            .AddJwtBearer(AuthSchemes.TempToken, options =>
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
                    OnTokenValidated = context =>
                    {
                        var tokenTypeClaim = context.Principal?.FindFirst(AuthClaims.TokenType)?.Value;
                        if (tokenTypeClaim != AuthTokenTypes.Temp)
                        {
                            context.Fail("Invalid token type. Expected a temp token.");
                            return Task.CompletedTask;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes(AuthSchemes.Default)
                .Build();

            options.AddPolicy(PolicyNames.TwoFactorAuthPolicy, policy =>
                 policy.RequireAuthenticatedUser()
                 .RequireClaim(AuthClaims.TokenType, AuthTokenTypes.Temp)
                 .AddAuthenticationSchemes(AuthSchemes.TempToken));

            options.AddPolicy(PolicyNames.HasShopClaimPolicy, policy =>
                 policy.RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(AuthSchemes.Default)
                    .RequireAssertion(context =>
                    {
                        var roleId = context.User.FindFirst(ClaimTypes.Role)?.Value;
                        if (roleId == SuperAdminConstants.SuperAdminRoleId)
                            return true;

                        return context.User.HasClaim(c => c.Type == AuthClaims.ShopId);
                    }));

            options.AddPolicy(PolicyNames.AdminOnlyPolicy, policy =>
                 policy.RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(AuthSchemes.Default)
                    .RequireAssertion(context =>
                    {
                        var roleId = context.User.FindFirst(ClaimTypes.Role)?.Value;
                        return roleId == RoleConstants.Admin.ToString() ||
                               roleId == SuperAdminConstants.SuperAdminRoleId;
                    }));
        });

        return services;
    }
}
