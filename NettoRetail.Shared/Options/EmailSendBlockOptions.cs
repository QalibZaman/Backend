namespace NettoRetail.Shared.Options;

public class EmailSendBlockOptions
{
    public required int BlockDurationInHours { get; set; }
    public required int SentEmailsThreshold { get; set; }
    public required int LockoutDurationInMinutes { get; set; }
}