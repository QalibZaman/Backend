using System.Text.Json.Serialization;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.InventoryItem;

public class DetailModel
{
    public Guid? Id { get; init; }
    public string? Name { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductDetailTypeEnum? TypeEnum { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProductDetailUnitEnum? Unit { get; init; }
    public string? Value { get; set; }
    public int? OrderIndex { get; set; }
    public bool IsDelete { get; set; }
}