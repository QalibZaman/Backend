using System.IdentityModel.Tokens.Jwt;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Entities;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Infrastructure.Models;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services.ISessionModelFactory;

public class SessionModelFactory(IStaffRepository accountRepository, ICustomerRepository customerRepository) : ISessionModelFactory
{
    public async Task<SessionModel> CreateSessionModel(
        string userAgentString,
        string ips,
        string jwt,
        string refreshToken,
        Location? location,
        DateTime expiresAt,
        UserTypeEnum userType,
        CancellationToken cancellationToken)
    {
        var accountEmail = GetEmailFromJwt(jwt);
        var deviceType = GetDeviceType(userAgentString);
        var browser = GetBrowser(userAgentString);

        var locationString = string.Empty;

        if (location != null)
        {
            locationString = $"{location.Country ?? "Unknown"}, {location.City ?? "Unknown"}";
        }
        else
        {
            locationString = "Unknown, Unknown";
        }

        //Ips may come in a format of string list, e.g. "0.0.0.0, 1.1.1.1, ..."
        var ip = ips.Split(',')[0].Trim();

        BaseUserEntity account;

        if (userType == UserTypeEnum.Admin)
        {
            var specification = new GetStaffByEmailSpecification(accountEmail);
            account = await accountRepository.GetAsync(specification, cancellationToken)
                ?? throw new InvalidTokenCredentialsException("Error trying adding a session with token's credentials"); ;
        }
        else
        {
            var specification = new GetCustomerByEmailAndShopIdSpecification(accountEmail, GetShopIdFromJwt(jwt));
            account = await customerRepository.GetAsync(specification, cancellationToken)
                ?? throw new InvalidTokenCredentialsException("Error trying adding a session with token's credentials"); ;
        }

        var accountGuid = account.Id;

        return new SessionModel
        {
            Id = Guid.NewGuid(),
            Jwt = jwt,
            RefreshToken = refreshToken,
            Device = deviceType,
            Browser = browser,
            IpAddress = ip,
            Location = locationString,
            StartTime = DateTime.UtcNow,
            LastActivityTime = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            AccountId = accountGuid,
        };
    }

    private static string GetDeviceType(string userAgent) =>
        userAgent switch
        {
            null => "Unknown",
            _ when userAgent.Contains("Mobile") => "Mobile",
            _ when userAgent.Contains("Tablet") => "Tablet",
            _ when userAgent.Contains("Windows") => "Windows PC",
            _ when userAgent.Contains("Macintosh") => "Mac",
            _ => "Unknown"
        };

    private static string GetEmailFromJwt(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(jwt))
        {
            throw new ArgumentException("Invalid JWT token");
        }

        var token = handler.ReadJwtToken(jwt);

        var emailClaim = token.Claims.FirstOrDefault(c => c.Type == "email");

        return emailClaim?.Value ?? "Unknown";
    }

    private static int GetShopIdFromJwt(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(jwt))
        {
            throw new ArgumentException("Invalid JWT token");
        }

        var token = handler.ReadJwtToken(jwt);

        var shopIdClaim = token.Claims.FirstOrDefault(c => c.Type == "shop_id");

        if (!int.TryParse(shopIdClaim.Value, out var shopId))
            throw new InvalidTokenCredentialsException("Shop ID in token is not a valid integer.");

        return shopId;
    }

    private static string GetBrowser(string userAgent) =>
        userAgent switch
        {
            null => "Unknown",
            _ when userAgent.Contains("Chrome") => "Chrome",
            _ when userAgent.Contains("Firefox") => "Firefox",
            _ when userAgent.Contains("Safari") && !userAgent.Contains("Chrome") => "Safari",
            _ when userAgent.Contains("Edge") => "Edge",
            _ when userAgent.Contains("MSIE") || userAgent.Contains("Trident") => "Internet Explorer",
            _ => "Unknown"
        };
}
