using NettoRetail.Domain.Entities.Enums;
using NettoRetail.Domain.Entities.JunctionEntities;

namespace NettoRetail.Application.Models.Response.Roles;

public class CreateRoleResponse
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public List<PermissionRoleResponse> Permissions { get; set; } = [];

    public class PermissionRoleResponse
    {
        public int PermissionId { get; set; }
        public AccessLevel AccessLevel { get; set; }
    }
}