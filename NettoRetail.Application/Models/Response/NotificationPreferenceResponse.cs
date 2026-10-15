namespace NettoRetail.Application.Models.Response;

public class NotificationPreferenceResponse
{
    public required int NotificationTypeId { get; set; }
    public bool IsEnabled { get; set; }
    public required string NotificationTypeCode { get; set; }
}