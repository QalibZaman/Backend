using NettoRetail.Domain.Entities.JunctionEntities;

namespace NettoRetail.Domain.Entities;

public class PermissionEntity
{
    public int Id { get; set; }
    public required string Title { get; set; }

    public ICollection<PermissionRoleEntity> PermissionRoles { get; set; } = null!;
}
