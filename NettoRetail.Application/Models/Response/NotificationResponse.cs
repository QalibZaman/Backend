namespace NettoRetail.Application.Models.Response;

public class NotificationResponse
{
    public required string Message { get; set; }
    public required string Topic { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public string? Url { get; set; }
}