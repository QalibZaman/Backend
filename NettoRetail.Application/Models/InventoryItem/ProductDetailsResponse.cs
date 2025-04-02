using NettoRetail.API.Infrastructure.JsonConverters;
using System.Text.Json.Serialization;

namespace NettoRetail.Application.Models.InventoryItem;
public class ProductDetailsResponse
{
    public Guid? Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public string? Unit { get; set; }

    [JsonConverter(typeof(ProductDetailsValueConverter))]
    public required object Value { get; set; }
    public required int OrderIndex { get; set; }
    public required Guid InventoryItemId { get; set; }
}
