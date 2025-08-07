namespace NettoRetail.Application.Services.PermissionCache;

public interface IPermissionCacheService
{
    IEnumerable<string>? GetPermissions(Guid userId);
    void SetPermissions(IEnumerable<string> permissions, Guid userId);
    void RemovePermissions(Guid userId);
    bool IsPermissionsExists(Guid userId);
}