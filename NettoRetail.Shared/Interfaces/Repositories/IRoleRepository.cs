using NettoRetail.Domain.Entities;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IRoleRepository : IBaseRepository<RoleEntity>
{
    Task<List<RoleEntity>> GetAllAsync(CancellationToken cancellationToken);
    Task<RoleEntity> AttachOrAddAsync(RoleEntity role, CancellationToken cancellationToken);
}
