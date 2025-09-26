using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Application.Models.Response
{
    public class AddToCartResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public int TotalQuantity { get; set; }
        public CartItemResponse? AddedItem { get; set; }
    }
}
