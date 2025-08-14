using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Response;

public class PlatformAdminResponse
{
    public Guid AccountId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? LastActivityTime { get; set; }
    public DateTime DateCreated { get; set; }
    public required string Email { get; set; } = string.Empty;
    public required StaffStatusEnum Status { get; set; }
    public required List<int> Permissions { get; set; }
}