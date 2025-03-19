using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class RoleEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }

    public int? ShopId { get; set; }
    public ShopEntity? Shop { get; set; }
    public ICollection<StaffEntity> StaffEntities { get; set; } = null!;
    public ICollection<PermissionRoleEntity> PermissionRoles { get; set; } = null!;
}