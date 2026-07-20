namespace NettoRetail.Domain.Entities;

public class SessionEntity
{
    public Guid Id { get; set; }
    public required string Jwt { get; set; }
    public required string RefreshToken { get; set; }
    public required string Device { get; set; }
    public required string Browser { get; set; }
    public required string IpAddress { get; set; }
    public required string Location { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime LastActivityTime { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Guid StaffId { get; set; }
    public StaffEntity Staff { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}
