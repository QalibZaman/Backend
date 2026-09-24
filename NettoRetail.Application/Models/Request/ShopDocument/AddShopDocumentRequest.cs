using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoRetail.Application.Models.Request.ShopDocument
{
    public class AddShopDocumentRequest
    {
        public IFormFile? RefundPolicy { get; set; }
        public IFormFile? ShippingPolicy { get; set; }

        public IFormFile? TermsAndConditions { get; set; }

        public IFormFile? PrivacyPolicy { get; set; }
    }
}
