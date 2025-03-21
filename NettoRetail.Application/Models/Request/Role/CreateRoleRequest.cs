namespace NettoRetail.Application.Models.Request.Role;

public class CreateRoleRequest
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required int[] PermissionIds { get; set; }
    public int ShopId { get; set; }
}
