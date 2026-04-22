namespace NettoRetail.Application.Models.Request.InventoryItem
{
    public record ToggleLinkProductsRequest(
        Guid ParentId,
        Guid ChildId,
        Dictionary<string, string>? ConflictResolutions = null);
}
