using System.Security.Claims;
using NettoRetail.Application.Exceptions;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal? principal)
    {
        var userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var isParsed = Guid.TryParse(userId, out Guid parsedUserId);

        return isParsed ?
            parsedUserId :
            throw new BadRequestException("User is unavailable");
    }

    public static int GetUserShopId(this ClaimsPrincipal? principal)
    {
        var role = principal?.FindFirstValue(ClaimTypes.Role);

        if (role == SuperAdminConstants.SuperAdminRoleId)
            return SuperAdminConstants.AllShopsIndicator;

        var userShopId = principal?.FindFirstValue(AuthClaims.ShopId);
        var isParsed = int.TryParse(userShopId, out int parsedUserShopId);

        return isParsed ?
            parsedUserShopId :
            throw new BadRequestException("User shop_id is unavailable");
    }

    public static int GetRoleId(this ClaimsPrincipal? principal)
    {
        var roleId = principal?.FindFirstValue(ClaimTypes.Role);
        var isParsed = int.TryParse(roleId, out int parsedRoleId);

        return isParsed ?
            parsedRoleId :
            throw new BadRequestException("role_id is unavailable");
    }

    public static bool IsTempToken(this ClaimsPrincipal? principal)
    {
        var tokenType = principal?.FindFirstValue(AuthClaims.TokenType);
        var isTempToken = tokenType == AuthTokenTypes.Temp;
        return isTempToken;
    }
}
