using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.DataAccess.Repositories
{
    public interface IRoleRepository : IBaseRepository<RoleEntity>
    {
        Task<List<RoleEntity>> GetAllAsync(CancellationToken cancellationToken);
    }
}
