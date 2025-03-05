namespace NettoRetail.Application.Models.Entities;

public class SessionModel
{
    public Guid Id { get; set; }
    public string? Jwt { get; set; }
    public string? RefreshToken { get; set; }
    public string? Device { get; set; }
    public string? Browser { get; set; }
    public string? IpAddress { get; set; }
    public string? Location { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime LastActivityTime { get; set; }
    public Guid AccountId { get; set; }
}
