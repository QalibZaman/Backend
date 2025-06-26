using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Order;

public class OrderHistoryResponse
{
    public int Id { get; set; }
    public required DateTime CreationDate { get; set; }
    public required string DeliveryType { get; set; }
    public required string OrderStatus { get; set; }
    public required string Creator { get; set; }
}
