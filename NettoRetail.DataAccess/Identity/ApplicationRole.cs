using Microsoft.AspNetCore.Identity;
using NettoRetail.Domain.Entities.JunctionEntities;

namespace NettoRetail.Domain.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public ICollection<PermissionRoleEntity> PermissionRoles { get; set; } = null!;
    public ICollection<StaffEntity> StaffMembers { get; set; } = [];
    public ICollection<CustomerEntity> Customers { get; set; } = [];
}