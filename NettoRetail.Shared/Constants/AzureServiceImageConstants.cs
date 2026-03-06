namespace NettoRetail.Shared.Constants;

public static class AzureServiceImageConstants
{
    public const string CONTAINER_NAME = "dot-net";
    public static string GetInventoryItemImagesPathPrefix(Guid inventoryItemId) => $"inventory-item/{inventoryItemId}/images";
}