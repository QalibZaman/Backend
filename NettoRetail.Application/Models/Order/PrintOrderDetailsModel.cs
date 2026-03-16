using NettoRetail.Application.Models.InventoryItem;

namespace NettoRetail.Application.Models.Order;

public class PrintOrderDetailsModel
{
    public required int Id { get; set; }
    public required DateTime CreationDate { get; set; }
    public string Creator { get; set; }
    public required Guid CustomerId { get; set; }
    public required string CustomerFirstName { get; set; }
    public required string CustomerLastName { get; set; }
    public required string CustomerEmail { get; set; }
    public required string OrderStatus { get; set; }
    public required string DeliveryType { get; set; }
    public ICollection<OrderStatusHistoryDto> StatusHistory { get; set; }
    public required int TotalProducts { get; set; }
    public int TotalQuantity
    {
        get => InventoryItems?.Sum(i => i.Quantity) ?? 0;
    }

    public required ICollection<InventoryItemDto> InventoryItems { get; set; }
    public required decimal Subtotal { get; set; }
    public required decimal TotalSale { get; set; }
    public required decimal TotalAmount { get; set; }
}