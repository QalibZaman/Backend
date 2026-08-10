using NettoRetail.Application.Exceptions;
using System.Security.Claims;

namespace NettoRetail.API.Infrastructure.Extensions
{
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
