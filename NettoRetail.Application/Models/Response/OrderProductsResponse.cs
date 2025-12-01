using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Response;

public class OrderProductsResponse
{
    public decimal Sale { get; private set; } = 0.0m;
    public decimal Subtotal { get; private set; }
    public decimal Total { get; private set; }
    public string DeliveryType { get; set; }
    public OrderStatusEnum OrderStatus { get; set; }
    public ICollection<OrderItemResponse> OrderItems { get; set; }

    public void SetPrices()
    {
        foreach (var item in OrderItems)
        {
            Subtotal += item.Price * item.Quantity;
            if (item.IsPromoted) Sale += item.PromotionPrice * item.Quantity;
            if (!item.IsPromoted) Sale += 0.0m;
            Total += item.Total;
        }
    }
}
