using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Request.Order;

public class UpdateOrderRequest
{
    public required int OrderId { get; set; }
    public required OrderStatusEnum Status { get; set; }
    public string? Comment { get; set; }
}
