using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Repositories
{
    public interface IRoleRepository : IBaseRepository<RoleEntity>
    {
        Task<List<RoleEntity>> GetAllAsync(CancellationToken cancellationToken);
        Task<List<RoleEntity>> GetRolesWithPermissionsBySpec(Specification<RoleEntity> specification, CancellationToken cancellationToken);
    }
}
