using NettoRetail.Application.Models.Order;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Response;

public class OrderPrintingResponse
{
    public required int Id { get; set; }
    public required DateOnly CreationDate { get; set; }
    public required Guid CustomerId { get; set; }
    public required string CustomerFirstName { get; set; }
    public required string CustomerLastName { get; set; }
    public required string CustomerEmail { get; set; }
    public required OrderStatusEnum OrderStatus { get; set; }
    public required OrderDeliveryTypeEnum DeliveryType { get; set; }
    public ICollection<OrderStatusHistoryDto>? StatusHistory { get; set; }
    public required int TotalProducts { get; set; }
    public required int TotalQuantity { get; set; }
    public required ICollection<OrderItemEntity> OrderItems { get; set; }
    public required decimal Subtotal { get; set; }
    public required decimal TotalSale { get; set; }
    public required decimal TotalAmount { get; set; }
}
