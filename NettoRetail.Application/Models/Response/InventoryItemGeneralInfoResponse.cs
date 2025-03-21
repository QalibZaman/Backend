using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Models.Response;

public class InventoryItemGeneralInfoResponse
{
    public required string ProductName { get; set; }
    public required long ProductUpc { get; set; }
    public required IEnumerable<string> ProductCategories { get; set; }
    public required decimal Price { get; set; }
    public required string Description { get; set; }
}
