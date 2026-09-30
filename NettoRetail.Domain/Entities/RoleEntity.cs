using NettoRetail.Domain.Entities.JunctionEntities;

namespace NettoRetail.Domain.Entities;

public class RoleEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? ShopEntityId { get; set; }
    public ICollection<PermissionRoleEntity> PermissionRoles { get; set; } = null!;
    public ICollection<StaffEntity> StaffMembers { get; set; } = [];
    public ICollection<CustomerEntity> Customers { get; set; } = [];
}