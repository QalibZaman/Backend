using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Domain.Entities
{
    public class CartItemEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CartId { get; set; }
        public CartEntity Cart { get; set; } = null!;
        public long ProductUpc { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? Size { get; set; } 
        public decimal? PromotionPrice { get; set; }

        public decimal GetEffectivePrice() => PromotionPrice ?? Price;
    }
}
