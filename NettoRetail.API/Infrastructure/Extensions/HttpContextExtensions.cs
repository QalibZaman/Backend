using NettoRetail.Application.Exceptions;

namespace NettoRetail.API.Infrastructure.Extensions;

public static class HttpContextExtensions
{
    public static Guid GetUserId(this HttpContext context)
    {
        return context?.User?.GetUserId()
            ?? throw new NotFoundException("User context is unavailable");
    }

    public static int getUserShopId(this HttpContext context)
    {
        return context?.User?.GetUserShopId()
            ?? throw new NotFoundException("User context is unavailable");
    }
}