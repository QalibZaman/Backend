using NettoRetail.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Application.Models.Request.Cart
{
    public class AddCartItemRequest
    {
        public long ProductUpc { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal? PromotionPrice { get; set; }
        public string? Size { get; set; }
    }

}
