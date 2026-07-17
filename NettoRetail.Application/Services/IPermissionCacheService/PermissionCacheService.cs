using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Options;

namespace NettoRetail.Application.Services.PermissionCache;

public class PermissionCacheService : IPermissionCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IOptions<JwtOptions> _jwtOptions;

    public PermissionCacheService(IMemoryCache memoryCache, IOptions<JwtOptions> jwtOptions)
    {
        _memoryCache = memoryCache;
        _jwtOptions = jwtOptions;
    }

    public IEnumerable<string>? GetPermissions(Guid sessionId)
    {
        var key = CreateCacheKey(sessionId);

        if (_memoryCache.TryGetValue(key, out IEnumerable<string>? permissions))
        {
            return permissions;
        }

        return [];
    }

    public void SetPermissions(IEnumerable<string> permissions, Guid sessionId)
    {
        var key = CreateCacheKey(sessionId);
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(_jwtOptions.Value.RefreshTokenExpirationDays),
        };
        _memoryCache.Set(key, permissions, options);
    }

    public void RemovePermissions(Guid sessionId)
    {
        var key = CreateCacheKey(sessionId);
        _memoryCache.Remove(key);
    }

    public bool IsPermissionsExists(Guid sessionId)
    {
        var key = CreateCacheKey(sessionId);
        return _memoryCache.TryGetValue(key, out IEnumerable<string>? permissions);
    }

    private string CreateCacheKey(Guid sessionId)
    {
        return $"{CacheServiceKeys.PERMISSIONS_KEY}-{sessionId}";
    }
}