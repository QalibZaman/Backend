using System.Security.Claims;
using NettoRetail.Application.Exceptions;

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
        var userShopId = principal?.FindFirstValue("shop_id");
        var isParsed = int.TryParse(userShopId, out int parsedUserShopId);

        return isParsed ?
            parsedUserShopId :
            throw new BadRequestException("User is unavailable");
    }
}
