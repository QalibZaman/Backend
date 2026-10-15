namespace NettoRetail.Application.Models.Response
{
    public class SessionResponse
    {
        public required string Id { get; set; }
        public string Device { get; set; } = "Unknown";
        public string Browser { get; set; } = "Unknown";
        public string IpAddress { get; set; } = "Unknown";
        public string Location { get; set; } = "Unknown, Unknown";
        public DateTime StartTime { get; set; }
        public DateTime LastActivityTime { get; set; }
        public bool IsCurrent { get; set; } = false;
    }
}
