namespace NettoRetail.Application.Models.Session;

public class SessionUpdateModel
{
    public string? PreviousJwt { get; set; }
    public required string CurrentJwt { get; set; }
    public string? CurrentRefreshToken { get; set; }
    public string? PreviousRefreshToken { get; set; }
    public DateTime LastActivityTime { get; set; }
    public DateTime ExpiresAt { get; set; }
}
