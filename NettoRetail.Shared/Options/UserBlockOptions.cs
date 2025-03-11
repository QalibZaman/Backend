namespace NettoRetail.Shared.Options;
public class UserBlockOptions
{
    public required int BlockDurationInHours { get; set; }
    public required int FailedAttemptThreshold { get; set; }
    public required int LockoutDurationInMinutes { get; set; }
}
