namespace NettoRetail.Application.Models.Order;

public sealed record OrderProductDto
{
    public Guid InventoryItemId { get; set; }
    public int Amount { get; set; }
}
