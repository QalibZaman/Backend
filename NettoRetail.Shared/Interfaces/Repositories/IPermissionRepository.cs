using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Shared.Interfaces.Repositories;

public interface IPermissionRepository : IBaseRepository<PermissionEntity>
{
    Task<IEnumerable<int>> GetIdsBySpecAsync(Specification<PermissionEntity> specification, CancellationToken cancellationToken);
}
