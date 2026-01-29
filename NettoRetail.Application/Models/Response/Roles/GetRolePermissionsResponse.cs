namespace NettoRetail.Application.Models.Response.Roles;

public class GetRolePermissionsResponse
{
    public int RoleId { get; set; }
    public string RoleName { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, bool> Permissions { get; set; }
}
