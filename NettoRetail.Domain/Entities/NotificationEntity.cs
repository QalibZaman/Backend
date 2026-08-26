namespace NettoRetail.Domain.Entities;

public class NotificationEntity
{
    public Guid Id { get; set; }
    public Guid StaffId { get; set; }
    public DateTime CreatedAt { get; set; }
    public required string Message { get; set; }
    public required string Topic { get; set; }
    public bool IsRead { get; set; }
    public StaffEntity Staff { get; set; }
    public string? Url { get; set; }
}