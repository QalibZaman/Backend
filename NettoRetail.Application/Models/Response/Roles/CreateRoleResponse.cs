namespace NettoRetail.Application.Models.Response.Roles;

public class CreateRoleResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? ShopId { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}
