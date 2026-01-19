using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Order;

public class OrderStatusHistoryDto
{
    public required Guid Id { get; set; }
    public required int OrderId { get; set; }
    public required OrderStatusEnum OldStatus { get; set; }
    public required OrderStatusEnum NewStatus { get; set; }
    public string? Comment { get; set; }
    public required DateTime UpdatedAt { get; set; }
    public required AccountDtoForPdf UpdatedBy { get; set; }
}
