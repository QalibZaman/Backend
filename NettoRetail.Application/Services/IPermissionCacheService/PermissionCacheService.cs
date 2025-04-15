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

    public IEnumerable<string>? GetPermissions()
    {
        if (_memoryCache.TryGetValue(CacheServiceKeys.PERMISSIONS_KEY, out IEnumerable<string>? permissions))
        {
            return permissions;
        }

        return [];
    }

    public void SetPermissions(IEnumerable<string> permissions)
    {
        _memoryCache.Set(CacheServiceKeys.PERMISSIONS_KEY, permissions);
    }

    public void RemovePermissions()
    {
        _memoryCache.Remove(CacheServiceKeys.PERMISSIONS_KEY);
    }

    public bool IsPermissionsExists()
    {
        return _memoryCache.TryGetValue(CacheServiceKeys.PERMISSIONS_KEY, out IEnumerable<string>? permissions);
    }
}