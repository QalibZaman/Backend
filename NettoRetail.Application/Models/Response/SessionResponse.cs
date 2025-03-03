namespace NettoRetail.Application.Models.Response;

public class SessionResponse
{
    public required string Id { get; set; }
    public string? Device { get; set; }
    public string? Browser { get; set; }
    public string? IpAddress { get; set; }
    public string? Location { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime LastActivityTime { get; set; }
}
