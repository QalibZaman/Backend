using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Request.Staff;

public class EditPlatformAdminRequest
{
  public List<PlatformAdminPermission> Permissions { get; init; } = [];
}
