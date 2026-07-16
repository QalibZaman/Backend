namespace NettoRetail.Shared.Options;

public class AdminRoleOptions
{
    public required int RoleId { get; set; }
    public required int[] PermissionIds { get; set; }
}
