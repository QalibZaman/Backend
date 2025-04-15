namespace NettoRetail.Application.Services.PermissionCache;

public interface IPermissionCacheService
{
    IEnumerable<string>? GetPermissions();
    void SetPermissions(IEnumerable<string> permissions);
    void RemovePermissions();
    bool IsPermissionsExists();
}