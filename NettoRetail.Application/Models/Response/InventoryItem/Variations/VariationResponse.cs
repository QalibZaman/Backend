namespace NettoRetail.Application.Models.Response.InventoryItem.Variations;

public class VariationResponse
{
    public Guid Id { get; set; }
    public required string ProductName { get; set; }
    public long ProductUpc { get; set; }
    public decimal Price { get; set; }
    public required string Description { get; set; }
    public Guid ParentId { get; set; }
    public bool Exists { get; set; }
    public bool IsActive { get; set; }
}
