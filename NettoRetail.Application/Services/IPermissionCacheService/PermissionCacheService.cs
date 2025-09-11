using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Application.Services.PermissionCache;

public class PermissionCacheService : IPermissionCacheService
{
    private readonly IMemoryCache _memoryCache;

    public PermissionCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public IEnumerable<string>? GetPermissions(Guid userId)
    {
        var key = CreateCacheKey(userId);

        if (_memoryCache.TryGetValue(key, out IEnumerable<string>? permissions))
        {
            return permissions;
        }

        return [];
    }

    public void SetPermissions(IEnumerable<string> permissions, Guid userId)
    {
        var key = CreateCacheKey(userId);
        _memoryCache.Set(key, permissions);
    }

    public void RemovePermissions(Guid userId)
    {
        var key = CreateCacheKey(userId);
        _memoryCache.Remove(key);
    }

    public bool IsPermissionsExists(Guid userId)
    {
        var key = CreateCacheKey(userId);
        return _memoryCache.TryGetValue(key, out IEnumerable<string>? permissions);
    }

    private string CreateCacheKey(Guid userId)
    {
        return $"{CacheServiceKeys.PERMISSIONS_KEY}-{userId}";
    }
}