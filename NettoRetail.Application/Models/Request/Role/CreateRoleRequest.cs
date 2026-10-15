using NettoRetail.DataAccess.DTO;

namespace NettoRetail.Application.Models.Request.Role;

public class CreateRoleRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required List<PermissionsDTO> Permissions { get; set; }
}
