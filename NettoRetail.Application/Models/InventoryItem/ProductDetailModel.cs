using NettoRetail.API.Infrastructure.JsonConverters;
using NettoRetail.Domain.Enums;
using System.Text.Json.Serialization;

namespace NettoRetail.Application.Models.InventoryItem;
public class ProductDetailModel
{
    public Guid? Id { get; set; }
    public required string Name { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required ProductDetailTypeEnum Type { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductDetailUnitEnum? Unit { get; set; }

    [JsonConverter(typeof(ProductDetailsValueConverter))]
    public required object Value { get; set; }
    public int OrderIndex { get; set; }
}
