using System.Text.Json.Serialization;
using NettoRetail.API.Infrastructure.JsonConverters;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.InventoryItem;

public interface IInventoryItemDetailModel
{
    public string Name { get; set; }
    public ProductDetailTypeEnum Type { get; set; }
    public ProductDetailUnitEnum? Unit { get; set; }
    public object Value { get; set; }
    public int OrderIndex { get; set; }
}
