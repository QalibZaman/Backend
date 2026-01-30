using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IRoleRepository : IBaseRepository<RoleEntity>
{
    Task<List<RoleEntity>> GetAllAsync(CancellationToken cancellationToken);
    Task<RoleEntity> AttachOrAddAsync(RoleEntity role, CancellationToken cancellationToken);
    Task<bool> TryLockAsync(int roleId, int shopId, Guid userId, DateTime nowUtc, TimeSpan ttl, CancellationToken cancellationToken);
    Task<bool> RefreshLockAsync(int roleId, int shopId, Guid userId, DateTime nowUtc, TimeSpan ttl, CancellationToken cancellationToken);
    Task<bool> UnlockAsync(int roleId, int shopId, Guid userId, CancellationToken cancellationToken);
}
