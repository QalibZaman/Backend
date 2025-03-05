namespace NettoRetail.Domain.Entities;

public class SessionEntity
{
    public Guid Id { get; set; }
    public required string Jwt { get; set; }
    public required string Refresh { get; set; }
    public required string Device { get; set; }
    public required string Browser { get; set; }
    public required string IpAddress { get; set; }
    public required string Location { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime LastActivityTime { get; set; }
    public Guid AccountId { get; set; } // currently fk to staff
    public required StaffEntity Account { get; set; }
}
