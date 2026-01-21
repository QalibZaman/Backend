using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IRoleRepository : IBaseRepository<RoleEntity>
{
    Task<List<RoleEntity>> GetAllAsync(CancellationToken cancellationToken);
    Task<RoleEntity> AttachOrAddAsync(RoleEntity role, CancellationToken cancellationToken);
}
