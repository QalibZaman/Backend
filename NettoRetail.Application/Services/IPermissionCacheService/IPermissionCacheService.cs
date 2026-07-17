namespace NettoRetail.Application.Services.PermissionCache;

public interface IPermissionCacheService
{
    IEnumerable<string>? GetPermissions(Guid sessionId);
    void SetPermissions(IEnumerable<string> permissions, Guid sessionId);
    void RemovePermissions(Guid sessionId);
    bool IsPermissionsExists(Guid sessionId);
}