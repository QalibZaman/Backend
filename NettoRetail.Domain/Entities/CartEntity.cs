using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Domain.Entities
{
    public class CartEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CustomerId { get; set; }
        public List<CartItemEntity> Items { get; set; } = new List<CartItemEntity>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        public int TotalQuantity => Items.Sum(i => i.Quantity);
        public decimal TotalPrice => Items.Sum(i => i.GetEffectivePrice() * i.Quantity);
    }
}
