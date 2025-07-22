namespace NettoRetail.Application.Models.Request.Staff;

public class EditPlatformAdminRequest
{
  public PlatformAdminPermissionRequest Permissions { get; set; } = new();
}
