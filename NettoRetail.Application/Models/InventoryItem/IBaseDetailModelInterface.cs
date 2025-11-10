using System.Text.Json.Serialization;
using NettoRetail.API.Infrastructure.JsonConverters;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.InventoryItem;

public interface IBaseDetailModelInterface
{
    public string Name { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductDetailTypeEnum Type { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductDetailUnitEnum? Unit { get; set; }

    [JsonConverter(typeof(ProductDetailsValueConverter))]
    public object Value { get; set; }
    public int OrderIndex { get; set; }
}
