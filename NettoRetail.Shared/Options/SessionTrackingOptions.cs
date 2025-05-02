namespace NettoRetail.Shared.Options;

public class SessionTrackingOptions
{
    public int InactivityTimeoutMinutes { get; set; }
    public int UpdateThresholdMinutes { get; set; }
}
