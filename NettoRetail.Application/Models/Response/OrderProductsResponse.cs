using NettoRetail.Domain.Enums;
using System.Text.Json.Serialization;

namespace NettoRetail.Application.Models.Response;

public class OrderProductsResponse
{
    public decimal Sale { get; set; } = 0.0m;
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required OrderDeliveryTypeEnum DeliveryType { get; set; }
    public ICollection<OrderItemResponse> OrderItems { get; set; } = new List<OrderItemResponse>();
}
