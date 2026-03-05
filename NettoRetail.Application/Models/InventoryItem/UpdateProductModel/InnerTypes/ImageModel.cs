using Microsoft.AspNetCore.Http;

namespace NettoRetail.Application.Models.InventoryItem;

// TODO: Extract it into another endpoint.
public class ImageModel
{
    public IFormFile? Image { get; init; }
    public string? Description { get; set; }
    public int? Order { get; init; }
}