using Microsoft.Azure.Cosmos;
using NettoRetail.Application.Exceptions;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Extensions;

public static class HttpContextExtensions
{
    public static Guid GetUserId(this HttpContext context)
    {
        return context?.User?.GetUserId()
            ?? throw new NotFoundException("User context is unavailable");
    }

    public static int GetUserShopId(this HttpContext context)
    {
        return context?.User?.GetUserShopId()
            ?? throw new NotFoundException("User context is unavailable");
    }

    public static int GetRoleId(this HttpContext context)
    {
        return context?.User?.GetRoleId()
            ?? throw new NotFoundException("User context is unavailable");
    }

    public static IEnumerable<string> GetPermissions(this HttpContext context)
    {
        return context.Items["Permissions"] as IEnumerable<string> ?? Enumerable.Empty<string>();
    }

    public static bool IsTempToken(this HttpContext context)
    {
        return context.User.IsTempToken();
    }
}