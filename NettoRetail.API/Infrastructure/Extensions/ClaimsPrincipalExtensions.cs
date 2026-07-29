using System.Security.Claims;
using NettoRetail.Application.Exceptions;
using System.Security.Claims;

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
}
}
