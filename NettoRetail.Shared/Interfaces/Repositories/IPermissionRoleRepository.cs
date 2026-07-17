using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IPermissionRoleRepository : IBaseRepository<PermissionRoleEntity>
{
    Task<IEnumerable<string>> GetPermissionsByRoleIdAsync(Specification<PermissionRoleEntity> specification, CancellationToken cancellationToken);
}
