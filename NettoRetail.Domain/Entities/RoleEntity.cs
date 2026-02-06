using NettoRetail.Domain.Entities.JunctionEntities;

namespace NettoRetail.Domain.Entities;

public class RoleEntity
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? ShopId { get; set; }
    public ShopEntity? Shop { get; set; }
    public ICollection<PermissionRoleEntity> PermissionRoles { get; set; } = null!;
    public ICollection<AccountEntity> Accounts { get; set; } = null!;
}