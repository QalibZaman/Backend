namespace NettoRetail.Application.Models.Request.Comparison;

public class AddComparisonItemRequest
{
    public required string UserId { get; set; }
    public required Guid InventoryItemId { get; set; }
}
