namespace NettoRetail.Application.Models.Request.Staff;

public class EditPlatformAdminRequest
{
  public List<string> AssignedPermissions { get; set; } = new();
}
