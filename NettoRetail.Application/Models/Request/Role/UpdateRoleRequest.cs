namespace NettoRetail.Application.Models.Request.Role;

public class UpdateRoleRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required int[] PermissionIds { get; set; }
}