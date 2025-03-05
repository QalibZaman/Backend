namespace NettoRetail.Application.Models.Session;

public class SessionUpdateModel
{
    public string? PreviousJwt { get; set; }
    public required string CurrentJwt { get; set; }
    public string? CurrentRefreshToken { get; set; } // should be required but idk how authorization would work so i left it nullable
    public DateTime LastActivityTime { get; set; }
}
