using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Response;

public class CreatePlatformAdminResponse
{
    public Guid AccountId { get; init; }
    public required string Email { get; init; } = string.Empty;
    public required StaffStatusEnum Status { get; init; }
    public required List<PlatformAdminPermission> Permissions { get; init; }
}