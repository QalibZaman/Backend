using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IRoleRepository : IBaseRepository<RoleEntity>
{
    Task<ICollection<RoleEntity>> GetAllPaginatedAsync(Specification<RoleEntity> specification, CancellationToken cancellationToken);
}