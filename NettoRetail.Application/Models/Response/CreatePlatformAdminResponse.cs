using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Response;

public class CreatePlatformAdminResponse
{
    public Guid AccountId { get; set; }
    public string Email { get; set; } = default!;
    public StaffStatusEnum Status { get; set; }
    public List<string> AssignedPermissions { get; set; } = new();
}