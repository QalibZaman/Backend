using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Application.Models.Response
{
    public class ProductAvailabilityResponse
    {
        public bool Success { get; set; }
        public bool InStock { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
