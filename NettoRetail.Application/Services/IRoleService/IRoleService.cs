using NettoRetail.Application.Models.Admin;
using NettoRetail.Domain.Entities;
namespace NettoRetail.Application.Services.IRoleService
{
    public interface IRoleService
    {
        public Task<RoleEntity> CreatePlatformAdminRole(CreatePlatformAdminModel model, CancellationToken cancellationToken);
    }
}
