using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.Application.Authentication;
using NettoRetail.Application.Exceptions;

namespace NettoRetail.API.Infrastructure.Helpers.Authentication;

internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Guid UserId => _httpContextAccessor
        .HttpContext?
        .User
        .GetUserId() ?? throw new BadRequestException("User context is unavailable");
}
